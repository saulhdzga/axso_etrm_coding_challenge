namespace Axso.PowerPositionReporter.Domain.Positions;

/// <summary>
/// Represents the aggregated volume for a single local clock hour in the report.
/// </summary>
/// <param name="LocalTime">The local time label for the report row.</param>
/// <param name="Volume">The aggregated trade volume for the hour.</param>
public sealed record HourlyPowerPosition(TimeOnly LocalTime, decimal Volume);
