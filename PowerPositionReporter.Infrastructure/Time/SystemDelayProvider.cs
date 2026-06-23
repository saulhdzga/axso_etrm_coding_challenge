using Axso.PowerPositionReporter.Application.Abstractions;

namespace Axso.PowerPositionReporter.Infrastructure.Time;

/// <summary>
/// Uses the system task scheduler for delays.
/// </summary>
public sealed class SystemDelayProvider : IDelayProvider
{
    /// <inheritdoc />
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
