using Axso.PowerPositionReporter.Domain.Positions;

namespace Axso.PowerPositionReporter.Application.Abstractions;

/// <summary>
/// Defines the output boundary for writing aggregated power position reports.
/// </summary>
public interface IPowerPositionCsvWriter
{
    /// <summary>
    /// Writes report rows to a timestamped CSV file.
    /// </summary>
    /// <param name="outputPath">Directory path where the CSV file should be created.</param>
    /// <param name="extractLocalTime">Local extract time used for the report filename.</param>
    /// <param name="positions">Aggregated hourly positions to include in the report.</param>
    /// <param name="cancellationToken">Token used to cancel the write operation.</param>
    /// <returns>A task that completes when the CSV file has been written.</returns>
    Task WriteAsync(
        string outputPath,
        DateTime extractLocalTime,
        IReadOnlyCollection<HourlyPowerPosition> positions,
        CancellationToken cancellationToken);
}
