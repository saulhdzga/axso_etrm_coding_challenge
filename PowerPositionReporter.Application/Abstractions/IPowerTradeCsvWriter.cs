using Axso.PowerPositionReporter.Domain.Trades;

namespace Axso.PowerPositionReporter.Application.Abstractions;

/// <summary>
/// Defines the output boundary for writing raw power trades for diagnostics.
/// </summary>
public interface IPowerTradeCsvWriter
{
    /// <summary>
    /// Writes raw trade period rows to a timestamped CSV file.
    /// </summary>
    /// <param name="outputPath">Directory path where the CSV file should be created.</param>
    /// <param name="extractLocalTime">Local extract time used for the file name.</param>
    /// <param name="trades">Raw trades to include in the diagnostic file.</param>
    /// <param name="cancellationToken">Token used to cancel the write operation.</param>
    /// <returns>The created file path.</returns>
    Task<string> WriteAsync(
        string outputPath,
        DateTime extractLocalTime,
        IReadOnlyCollection<PowerTradePosition> trades,
        CancellationToken cancellationToken);
}
