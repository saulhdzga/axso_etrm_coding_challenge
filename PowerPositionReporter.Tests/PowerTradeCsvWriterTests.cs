using Axso.PowerPositionReporter.Infrastructure.Csv;
using Axso.PowerPositionReporter.Tests.TestData;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class PowerTradeCsvWriterTests
{
    /// <summary>
    /// Verifies that raw trade periods are written to the diagnostic CSV.
    /// </summary>
    [Fact]
    public async Task WriteAsync_WritesRawTradePeriodRows()
    {
        string outputPath = Path.Combine(Path.GetTempPath(), "PowerPositionReporterTests", Guid.NewGuid().ToString("N"));
        DateTime extractLocalTime = new(2026, 06, 22, 00, 08, 00);

        try
        {
            PowerTradeCsvWriter writer = new();

            string filePath = await writer.WriteAsync(
                outputPath,
                extractLocalTime,
                [
                    TestPowerTrades.CreateDomainTrade(
                        new DateTime(2026, 06, 22),
                        [10m, -5m],
                        tradeId: "trade-1"),
                ],
                TestContext.Current.CancellationToken);

            Assert.Equal(Path.Combine(outputPath, "PowerTrades_20260622_0008.csv"), filePath);

            string[] lines = await File.ReadAllLinesAsync(filePath, TestContext.Current.CancellationToken);
            Assert.Equal(
                [
                    "Trade Id,Trade Date,Period,Volume",
                    "trade-1,2026-06-22,1,10",
                    "trade-1,2026-06-22,2,-5",
                ],
                lines);
        }
        finally
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, recursive: true);
            }
        }
    }
}
