using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Application.Abstractions;
using System.Globalization;

namespace Axso.PowerPositionReporter.Infrastructure.Csv;

/// <summary>
/// Writes aggregated power positions to CSV output.
/// </summary>
public sealed class PowerPositionCsvWriter : IPowerPositionCsvWriter
{
    /// <inheritdoc />
    public async Task WriteAsync(
        string outputPath,
        DateTime extractLocalTime,
        IReadOnlyCollection<HourlyPowerPosition> positions,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputPath);

        string filePath = Path.Combine(outputPath, GetFileName(extractLocalTime));

        await using FileStream fileStream = File.Create(filePath);
        await using StreamWriter writer = new(fileStream);

        await writer.WriteLineAsync("Local Time,Volume");

        foreach (HourlyPowerPosition position in positions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string line = string.Create(
                CultureInfo.InvariantCulture,
                $"{position.LocalTime:HH:mm},{position.Volume}");

            await writer.WriteLineAsync(line);
        }
    }

    /// <summary>
    /// Generates a timestamped filename for the CSV report based on the local extract time.
    /// </summary>
    /// <param name="extractLocalTime">Local extract time to include in the filename.</param>
    /// <returns>The report filename.</returns>
    private static string GetFileName(DateTime extractLocalTime)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"PowerPosition_{extractLocalTime:yyyyMMdd_HHmm}.csv");
    }
}
