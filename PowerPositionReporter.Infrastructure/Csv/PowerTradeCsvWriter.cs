using Axso.PowerPositionReporter.Application.Abstractions;
using Axso.PowerPositionReporter.Domain.Trades;
using System.Globalization;

namespace Axso.PowerPositionReporter.Infrastructure.Csv;

/// <summary>
/// Writes raw power trades to CSV output for diagnostic verification.
/// </summary>
public sealed class PowerTradeCsvWriter : IPowerTradeCsvWriter
{
    /// <inheritdoc />
    public async Task<string> WriteAsync(
        string outputPath,
        DateTime extractLocalTime,
        IReadOnlyCollection<PowerTradePosition> trades,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputPath);

        string filePath = Path.Combine(outputPath, GetFileName(extractLocalTime));

        await using FileStream fileStream = File.Create(filePath);
        await using StreamWriter writer = new(fileStream);

        await writer.WriteLineAsync("Trade Id,Trade Date,Period,Volume");

        foreach (PowerTradePosition trade in trades)
        {
            foreach (PowerPeriodPosition period in trade.Periods)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string line = string.Create(
                    CultureInfo.InvariantCulture,
                    $"{trade.TradeId},{trade.Date:yyyy-MM-dd},{period.Period},{period.Volume}");

                await writer.WriteLineAsync(line);
            }
        }

        return filePath;
    }

    /// <summary>
    /// Generates a timestamped filename for the raw trade diagnostic CSV.
    /// </summary>
    /// <param name="extractLocalTime">Local extract time to include in the filename.</param>
    /// <returns>The diagnostic filename.</returns>
    private static string GetFileName(DateTime extractLocalTime)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"PowerTrades_{extractLocalTime:yyyyMMdd_HHmm}.csv");
    }
}
