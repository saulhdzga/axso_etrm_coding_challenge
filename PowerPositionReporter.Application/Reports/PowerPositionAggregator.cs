using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Domain.Trades;
namespace Axso.PowerPositionReporter.Application.Reports;

/// <summary>
/// Aggregates raw power trades into hourly local-time power positions.
/// </summary>
public sealed class PowerPositionAggregator : IPowerPositionAggregator
{
    private const int PeriodsPerDay = 24;
    private const int FirstPeriodStartHour = 23;

    /// <inheritdoc />
    public IReadOnlyCollection<HourlyPowerPosition> Aggregate(IReadOnlyCollection<PowerTradePosition> trades)
    {
        Dictionary<int, decimal> volumeByPeriod = trades
            .SelectMany(trade => trade.Periods)
            .GroupBy(period => period.Period)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(period => (decimal)period.Volume));

        return [.. Enumerable
            .Range(1, PeriodsPerDay)
            .Select(period => new HourlyPowerPosition(
                LocalTime: GetLocalTime(period),
                Volume: volumeByPeriod.GetValueOrDefault(period)))];
    }

    /// <summary>
    /// Calculates the local time for the given period number.
    /// </summary>
    /// <param name="period">One-based power period number.</param>
    /// <returns>The local clock time represented by the period.</returns>
    private static TimeOnly GetLocalTime(int period)
    {
        int hour = (FirstPeriodStartHour + period - 1) % PeriodsPerDay;

        return new TimeOnly(hour, minute: 0);
    }
}
