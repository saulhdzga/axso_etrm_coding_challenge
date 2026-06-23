using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Infrastructure.Csv;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class PowerPositionCsvWriterTests
{
    /// <summary>
    /// Verifies that aggregated positions are written with the expected filename and CSV rows.
    /// </summary>
    [Fact]
    public async Task WriteAsync_WritesExpectedCsvFile()
    {
        string outputPath = Path.Combine(Path.GetTempPath(), "PowerPositionReporterTests", Guid.NewGuid().ToString("N"));
        DateTime extractLocalTime = new(2026, 06, 21, 18, 37, 00);
        HourlyPowerPosition[] positions =
        [
            new(new TimeOnly(23, 00), 150m),
            new(new TimeOnly(00, 00), 150.5m),
            new(new TimeOnly(10, 00), -20m),
        ];

        try
        {
            PowerPositionCsvWriter writer = new();

            await writer.WriteAsync(outputPath, extractLocalTime, positions, TestContext.Current.CancellationToken);

            string filePath = Path.Combine(outputPath, "PowerPosition_20260621_1837.csv");
            Assert.True(File.Exists(filePath));

            string[] lines = await File.ReadAllLinesAsync(filePath, TestContext.Current.CancellationToken);
            Assert.Equal(
                [
                    "Local Time,Volume",
                    "23:00,150",
                    "00:00,150.5",
                    "10:00,-20",
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

    /// <summary>
    /// Verifies that an empty position collection writes only the CSV header.
    /// </summary>
    [Fact]
    public async Task WriteAsync_WritesHeaderOnlyWhenThereAreNoPositions()
    {
        string outputPath = Path.Combine(Path.GetTempPath(), "PowerPositionReporterTests", Guid.NewGuid().ToString("N"));
        DateTime extractLocalTime = new(2026, 06, 21, 18, 37, 00);

        try
        {
            PowerPositionCsvWriter writer = new();

            await writer.WriteAsync(outputPath, extractLocalTime, [], TestContext.Current.CancellationToken);

            string filePath = Path.Combine(outputPath, "PowerPosition_20260621_1837.csv");
            string[] lines = await File.ReadAllLinesAsync(filePath, TestContext.Current.CancellationToken);

            Assert.Equal(["Local Time,Volume"], lines);
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
