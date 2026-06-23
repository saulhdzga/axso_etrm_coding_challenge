using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Domain.Trades;

namespace Axso.PowerPositionReporter.Application.Reports;

/// <summary>
/// Defines the aggregation boundary for converting retrieved trades into hourly report positions.
/// </summary>
public interface IPowerPositionAggregator
{
    /// <summary>
    /// Aggregates all power positions for the requested position date.
    /// </summary>
    /// <param name="trades">Raw trades to aggregate.</param>
    /// <returns>Hourly local-time positions ready for report output.</returns>
    IReadOnlyCollection<HourlyPowerPosition> Aggregate(IReadOnlyCollection<PowerTradePosition> trades);
}
