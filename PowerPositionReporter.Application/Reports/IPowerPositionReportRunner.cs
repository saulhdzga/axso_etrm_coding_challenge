namespace Axso.PowerPositionReporter.Application.Reports;

/// <summary>
/// Defines the orchestration boundary for producing a power position report extract.
/// </summary>
public interface IPowerPositionReportRunner
{
    /// <summary>
    /// Runs a single report extract.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the extract.</param>
    /// <returns>A task that completes when the extract has finished.</returns>
    Task RunOnceAsync(CancellationToken cancellationToken);
}
