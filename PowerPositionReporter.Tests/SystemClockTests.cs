using Axso.PowerPositionReporter.Infrastructure.Time;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class SystemClockTests
{
    /// <summary>
    /// Verifies that the system clock returns Greenwich Mean Time during winter.
    /// </summary>
    [Fact]
    public void GetLocalNow_ReturnsLondonTimeDuringWinter()
    {
        SystemClock clock = new(new FixedTimeProvider(new DateTimeOffset(2026, 01, 15, 12, 00, 00, TimeSpan.Zero)));

        DateTime localNow = clock.GetLocalNow();

        Assert.Equal(new DateTime(2026, 01, 15, 12, 00, 00), localNow);
    }

    /// <summary>
    /// Verifies that the system clock returns British Summer Time during summer.
    /// </summary>
    [Fact]
    public void GetLocalNow_ReturnsLondonTimeDuringSummer()
    {
        SystemClock clock = new(new FixedTimeProvider(new DateTimeOffset(2026, 06, 21, 12, 00, 00, TimeSpan.Zero)));

        DateTime localNow = clock.GetLocalNow();

        Assert.Equal(new DateTime(2026, 06, 21, 13, 00, 00), localNow);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
