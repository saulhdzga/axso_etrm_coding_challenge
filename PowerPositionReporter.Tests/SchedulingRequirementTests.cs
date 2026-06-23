using Axso.PowerPositionReporter.Application.Abstractions;
using Axso.PowerPositionReporter.Application.Configuration;
using Axso.PowerPositionReporter.Application.Reports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class SchedulingRequirementTests
{
    /// <summary>
    /// Verifies that the scheduler runs an extract immediately when it starts.
    /// </summary>
    [Fact]
    public async Task SchedulerRunsExtractWhenItStarts()
    {
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(5));
        FakeClock clock = new(new DateTime(2026, 06, 21, 23, 00, 00));
        Mock<IPowerPositionReportRunner> runner = new();
        runner
            .Setup(currentRunner => currentRunner.RunOnceAsync(cancellationTokenSource.Token))
            .Callback(cancellationTokenSource.Cancel)
            .Returns(Task.CompletedTask);

        PowerPositionScheduler scheduler = CreateScheduler(
            runner.Object,
            clock,
            new FakeDelayProvider(clock, cancellationTokenSource));

        await scheduler.RunAsync(cancellationTokenSource.Token);

        runner.Verify(
            currentRunner => currentRunner.RunOnceAsync(cancellationTokenSource.Token),
            Times.Once);
    }

    /// <summary>
    /// Verifies that the scheduler waits for the configured interval between extracts.
    /// </summary>
    [Fact]
    public async Task SchedulerRunsExtractsAtConfiguredInterval()
    {
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(5));
        FakeClock clock = new(new DateTime(2026, 06, 21, 23, 00, 00));
        FakeDelayProvider delayProvider = new(clock, cancellationTokenSource, cancelAfterDelays: 2);
        Mock<IPowerPositionReportRunner> runner = new();
        runner
            .Setup(currentRunner => currentRunner.RunOnceAsync(cancellationTokenSource.Token))
            .Returns(Task.CompletedTask);

        PowerPositionScheduler scheduler = CreateScheduler(runner.Object, clock, delayProvider);

        await scheduler.RunAsync(cancellationTokenSource.Token);

        Assert.Equal(
            [TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15)],
            delayProvider.RequestedDelays);

        runner.Verify(
            currentRunner => currentRunner.RunOnceAsync(cancellationTokenSource.Token),
            Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that the scheduler catches up when an extract runs past the next due time.
    /// </summary>
    [Fact]
    public async Task SchedulerDoesNotMissScheduledExtractsWhenPreviousExtractRunsLong()
    {
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(5));
        FakeClock clock = new(new DateTime(2026, 06, 21, 23, 00, 00));
        FakeDelayProvider delayProvider = new(clock, cancellationTokenSource, cancelAfterDelays: 1);
        int runCount = 0;
        Mock<IPowerPositionReportRunner> runner = new();
        runner
            .Setup(currentRunner => currentRunner.RunOnceAsync(cancellationTokenSource.Token))
            .Callback(() =>
            {
                runCount++;

                if (runCount == 1)
                {
                    clock.Advance(TimeSpan.FromMinutes(40));
                }
            })
            .Returns(Task.CompletedTask);

        PowerPositionScheduler scheduler = CreateScheduler(runner.Object, clock, delayProvider);

        await scheduler.RunAsync(cancellationTokenSource.Token);

        runner.Verify(
            currentRunner => currentRunner.RunOnceAsync(cancellationTokenSource.Token),
            Times.Exactly(3));
    }

    /// <summary>
    /// Creates a scheduler with test-controlled dependencies.
    /// </summary>
    /// <param name="runner">Report runner used by the scheduler.</param>
    /// <param name="clock">Clock used by the scheduler.</param>
    /// <param name="delayProvider">Delay provider used by the scheduler.</param>
    /// <returns>A scheduler configured for tests.</returns>
    private static PowerPositionScheduler CreateScheduler(
        IPowerPositionReportRunner runner,
        IClock clock,
        IDelayProvider delayProvider)
    {
        return new PowerPositionScheduler(
            runner,
            new ReportSettings("reports", 15),
            clock,
            delayProvider,
            NullLogger<PowerPositionScheduler>.Instance);
    }

    private sealed class FakeClock(DateTime localNow) : IClock
    {
        private DateTime _localNow = localNow;

        /// <inheritdoc />
        public DateTime GetLocalNow()
        {
            return _localNow;
        }

        /// <summary>
        /// Advances the fake local clock by the supplied duration.
        /// </summary>
        /// <param name="timeSpan">Duration to add to the current fake local time.</param>
        public void Advance(TimeSpan timeSpan)
        {
            _localNow = _localNow.Add(timeSpan);
        }
    }

    private sealed class FakeDelayProvider(
        FakeClock clock,
        CancellationTokenSource cancellationTokenSource,
        int cancelAfterDelays = int.MaxValue) : IDelayProvider
    {
        private int _delayCount;

        /// <summary>
        /// Gets the delays requested by the scheduler.
        /// </summary>
        public List<TimeSpan> RequestedDelays { get; } = [];

        /// <inheritdoc />
        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _delayCount++;
            RequestedDelays.Add(delay);
            clock.Advance(delay);

            if (_delayCount >= cancelAfterDelays)
            {
                cancellationTokenSource.Cancel();
            }

            return Task.CompletedTask;
        }
    }
}
