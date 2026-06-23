using Axso.PowerPositionReporter.Application.Reports;
using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Domain.Trades;
using Axso.PowerPositionReporter.Tests.TestData;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class PowerPositionAggregatorTests
{
    /// <summary>
    /// Verifies that volumes are summed by period and mapped to the expected local hours.
    /// </summary>
    [Fact]
    public void Aggregate_SumsVolumesAndMapsPeriodsToLocalHours()
    {
        DateTime positionDate = new(2026, 06, 21);
        PowerTradePosition tradeOne = TestPowerTrades.CreateDomainTrade(
            positionDate,
            [.. Enumerable.Repeat(100m, 24)],
            tradeId: "trade-1");
        PowerTradePosition tradeTwo = TestPowerTrades.CreateDomainTrade(
            positionDate,
            [.. Enumerable.Range(1, 24).Select(period => period <= 11 ? 50m : -20m)],
            tradeId: "trade-2");

        PowerPositionAggregator aggregator = new();

        HourlyPowerPosition[] positions = [.. aggregator.Aggregate([tradeOne, tradeTwo])];

        Assert.Equal(24, positions.Length);
        Assert.Equal(new TimeOnly(23, 0), positions[0].LocalTime);
        Assert.Equal(150m, positions[0].Volume);
        Assert.Equal(new TimeOnly(0, 0), positions[1].LocalTime);
        Assert.Equal(new TimeOnly(9, 0), positions[10].LocalTime);
        Assert.Equal(150m, positions[10].Volume);
        Assert.Equal(new TimeOnly(10, 0), positions[11].LocalTime);
        Assert.Equal(80m, positions[11].Volume);
        Assert.Equal(new TimeOnly(22, 0), positions[23].LocalTime);
        Assert.Equal(80m, positions[23].Volume);

        decimal[] expectedVolumes =
        [
            150m,
            150m,
            150m,
            150m,
            150m,
            150m,
            150m,
            150m,
            150m,
            150m,
            150m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
            80m,
        ];

        Assert.Equal(expectedVolumes, positions.Select(position => position.Volume));
    }

    /// <summary>
    /// Verifies that periods without trades are still emitted with zero volume.
    /// </summary>
    [Fact]
    public void Aggregate_FillsMissingPeriodsWithZeroVolume()
    {
        PowerPositionAggregator aggregator = new();

        HourlyPowerPosition[] positions = [.. aggregator.Aggregate([])];

        Assert.Equal(24, positions.Length);
        Assert.All(positions, position => Assert.Equal(0m, position.Volume));
    }

    /// <summary>
    /// Verifies the complete local wall-clock sequence for a power position day.
    /// </summary>
    [Fact]
    public void Aggregate_MapsAllPeriodsFromPreviousDay2300ToCurrentDay2200()
    {
        PowerPositionAggregator aggregator = new();

        HourlyPowerPosition[] positions = [.. aggregator.Aggregate([])];

        TimeOnly[] expectedLocalTimes =
        [
            new(23, 0),
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(3, 0),
            new(4, 0),
            new(5, 0),
            new(6, 0),
            new(7, 0),
            new(8, 0),
            new(9, 0),
            new(10, 0),
            new(11, 0),
            new(12, 0),
            new(13, 0),
            new(14, 0),
            new(15, 0),
            new(16, 0),
            new(17, 0),
            new(18, 0),
            new(19, 0),
            new(20, 0),
            new(21, 0),
            new(22, 0),
        ];

        Assert.Equal(expectedLocalTimes, positions.Select(position => position.LocalTime));
    }
}
