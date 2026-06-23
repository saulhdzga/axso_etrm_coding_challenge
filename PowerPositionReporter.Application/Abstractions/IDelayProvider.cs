namespace Axso.PowerPositionReporter.Application.Abstractions;

/// <summary>
/// Provides asynchronous delays for scheduling.
/// </summary>
public interface IDelayProvider
{
    /// <summary>
    /// Waits for the requested delay.
    /// </summary>
    /// <param name="delay">Amount of time to wait.</param>
    /// <param name="cancellationToken">Token used to cancel the delay.</param>
    /// <returns>A task that completes when the delay has elapsed.</returns>
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
