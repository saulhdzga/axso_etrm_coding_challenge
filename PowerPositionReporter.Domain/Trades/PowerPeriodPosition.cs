namespace Axso.PowerPositionReporter.Domain.Trades;

/// <summary>
/// Represents a raw power position for a trading period.
/// </summary>
/// <param name="Period">One-based power period number.</param>
/// <param name="Volume">Trade volume for the period.</param>
public sealed record PowerPeriodPosition(int Period, decimal Volume);
