using Axso.PowerPositionReporter.Application.Abstractions;
using Axso.PowerPositionReporter.Application.Configuration;
using Axso.PowerPositionReporter.Application.Reports;
using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Domain.Trades;
using Axso.PowerPositionReporter.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class PowerPositionReportRunnerTests
{
    /// <summary>
    /// Verifies that a report run retrieves trades, aggregates them, and writes both CSV outputs.
    /// </summary>
    [Fact]
    public async Task RunOnceAsync_AggregatesForExtractDateAndWritesReport()
    {
        DateTime extractLocalTime = new(2026, 06, 21, 18, 37, 00);
        ReportSettings settings = new("reports", 15);
        HourlyPowerPosition[] positions =
        [
            new(new TimeOnly(23, 00), 150m),
        ];
        PowerTradePosition[] trades =
        [
            TestPowerTrades.CreateDomainTrade(
                extractLocalTime.Date,
                [.. Enumerable.Repeat(150m, 24)]),
        ];

        Mock<IClock> clock = new();
        clock
            .Setup(currentClock => currentClock.GetLocalNow())
            .Returns(extractLocalTime);

        Mock<IPowerPositionAggregator> aggregator = new();
        aggregator
            .Setup(currentAggregator => currentAggregator.Aggregate(trades))
            .Returns(positions);

        Mock<IPowerTradeProvider> tradeProvider = new();
        tradeProvider
            .Setup(currentTradeProvider => currentTradeProvider.GetTradesAsync(extractLocalTime.Date, CancellationToken.None))
            .ReturnsAsync(trades);

        Mock<IPowerTradeCsvWriter> tradeWriter = new();
        tradeWriter
            .Setup(currentTradeWriter => currentTradeWriter.WriteAsync(
                settings.OutputPath,
                extractLocalTime,
                trades,
                CancellationToken.None))
            .ReturnsAsync("reports/PowerTrades_20260621_1837.csv");

        Mock<IPowerPositionCsvWriter> writer = new();
        Mock<ILogger<PowerPositionReportRunner>> logger = new();

        PowerPositionReportRunner runner = new(
            aggregator.Object,
            tradeProvider.Object,
            tradeWriter.Object,
            writer.Object,
            clock.Object,
            settings,
            logger.Object);

        await runner.RunOnceAsync(CancellationToken.None);

        aggregator.Verify(
            currentAggregator => currentAggregator.Aggregate(trades),
            Times.Once);

        tradeWriter.Verify(
            currentTradeWriter => currentTradeWriter.WriteAsync(
                settings.OutputPath,
                extractLocalTime,
                trades,
                CancellationToken.None),
            Times.Once);

        writer.Verify(
            currentWriter => currentWriter.WriteAsync(
                settings.OutputPath,
                extractLocalTime,
                positions,
                CancellationToken.None),
            Times.Once);
    }

    /// <summary>
    /// Verifies that aggregated report output is not written when aggregation fails.
    /// </summary>
    [Fact]
    public async Task RunOnceAsync_DoesNotWriteReportWhenAggregationFails()
    {
        DateTime extractLocalTime = new(2026, 06, 21, 18, 37, 00);
        InvalidOperationException expectedException = new("Aggregation failed.");

        Mock<IClock> clock = new();
        clock
            .Setup(currentClock => currentClock.GetLocalNow())
            .Returns(extractLocalTime);

        Mock<IPowerPositionAggregator> aggregator = new();
        aggregator
            .Setup(currentAggregator => currentAggregator.Aggregate(It.IsAny<IReadOnlyCollection<PowerTradePosition>>()))
            .Throws(expectedException);

        Mock<IPowerTradeProvider> tradeProvider = new();
        tradeProvider
            .Setup(currentTradeProvider => currentTradeProvider.GetTradesAsync(extractLocalTime.Date, CancellationToken.None))
            .ReturnsAsync(
            [
                TestPowerTrades.CreateDomainTrade(
                    extractLocalTime.Date,
                    [.. Enumerable.Repeat(150m, 24)]),
            ]);

        Mock<IPowerTradeCsvWriter> tradeWriter = new();
        tradeWriter
            .Setup(currentTradeWriter => currentTradeWriter.WriteAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<IReadOnlyCollection<PowerTradePosition>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("reports/PowerTrades_20260621_1837.csv");

        Mock<IPowerPositionCsvWriter> writer = new();
        Mock<ILogger<PowerPositionReportRunner>> logger = new();

        PowerPositionReportRunner runner = new(
            aggregator.Object,
            tradeProvider.Object,
            tradeWriter.Object,
            writer.Object,
            clock.Object,
            new ReportSettings("reports", 15),
            logger.Object);

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.RunOnceAsync(CancellationToken.None));

        Assert.Same(expectedException, actualException);
        writer.Verify(
            currentWriter => currentWriter.WriteAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<IReadOnlyCollection<HourlyPowerPosition>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
