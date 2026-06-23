namespace Axso.PowerPositionReporter.Application.Abstractions;

/// <summary>
/// Provides the current time for report orchestration.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current local time used by the report extract.
    /// </summary>
    /// <returns>The current local time.</returns>
    DateTime GetLocalNow();
}
