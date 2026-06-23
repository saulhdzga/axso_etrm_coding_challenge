namespace Axso.PowerPositionReporter.Domain.Trades;

/// <summary>
/// Represents the raw period positions for a single power trade.
/// </summary>
/// <param name="TradeId">Identifier of the source trade.</param>
/// <param name="Date">Position date for the trade.</param>
/// <param name="Periods">Period positions included in the trade.</param>
public sealed record PowerTradePosition(
    string TradeId,
    DateTime Date,
    IReadOnlyCollection<PowerPeriodPosition> Periods);
