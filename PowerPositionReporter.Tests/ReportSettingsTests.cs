using Axso.PowerPositionReporter.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class ReportSettingsTests
{
    /// <summary>
    /// Verifies that report settings are read from configuration.
    /// </summary>
    [Fact]
    public void Load_UsesConfigurationValues()
    {
        IConfiguration configuration = CreateConfiguration(
            outputPath: "configured-output",
            intervalMinutes: "20");

        ReportSettings settings = ReportSettings.Load(configuration, []);

        Assert.Equal("configured-output", settings.OutputPath);
        Assert.Equal(20, settings.IntervalMinutes);
    }

    /// <summary>
    /// Verifies that command-line arguments override configured values.
    /// </summary>
    [Fact]
    public void Load_UsesCommandLineOverrides()
    {
        IConfiguration configuration = CreateConfiguration(
            outputPath: "configured-output",
            intervalMinutes: "20");

        ReportSettings settings = ReportSettings.Load(configuration, ["cli-output", "45"]);

        Assert.Equal("cli-output", settings.OutputPath);
        Assert.Equal(45, settings.IntervalMinutes);
    }

    /// <summary>
    /// Verifies that command-line arguments can provide required values without configuration.
    /// </summary>
    [Fact]
    public void Load_UsesCommandLineValuesWhenConfigurationIsMissing()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        ReportSettings settings = ReportSettings.Load(configuration, ["cli-output", "45"]);

        Assert.Equal("cli-output", settings.OutputPath);
        Assert.Equal(45, settings.IntervalMinutes);
    }

    /// <summary>
    /// Verifies that a missing output path is rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsMissingOutputPath()
    {
        IConfiguration configuration = CreateConfiguration(
            outputPath: "",
            intervalMinutes: "20");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => ReportSettings.Load(configuration, []));

        Assert.Equal("Report:OutputPath is required.", exception.Message);
    }

    /// <summary>
    /// Verifies that a missing interval is rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsMissingInterval()
    {
        IConfiguration configuration = CreateConfiguration(
            outputPath: "configured-output",
            intervalMinutes: "");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => ReportSettings.Load(configuration, []));

        Assert.Equal("Report:IntervalMinutes must be a valid integer.", exception.Message);
    }

    /// <summary>
    /// Verifies that an invalid command-line interval is rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsInvalidCommandLineInterval()
    {
        IConfiguration configuration = CreateConfiguration(
            outputPath: "configured-output",
            intervalMinutes: "20");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => ReportSettings.Load(configuration, ["cli-output", "not-a-number"]));

        Assert.Equal("command-line interval must be a valid integer.", exception.Message);
    }

    /// <summary>
    /// Verifies that a non-positive report interval is rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsNonPositiveInterval()
    {
        IConfiguration configuration = CreateConfiguration(
            outputPath: "configured-output",
            intervalMinutes: "0");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => ReportSettings.Load(configuration, []));

        Assert.Equal("Report interval must be greater than zero minutes.", exception.Message);
    }

    /// <summary>
    /// Creates an in-memory configuration for report settings tests.
    /// </summary>
    /// <param name="outputPath">Configured output path.</param>
    /// <param name="intervalMinutes">Configured interval in minutes.</param>
    /// <returns>The in-memory configuration.</returns>
    private static IConfiguration CreateConfiguration(string outputPath, string intervalMinutes)
    {
        KeyValuePair<string, string?>[] values =
        [
            KeyValuePair.Create<string, string?>("Report:OutputPath", outputPath),
            KeyValuePair.Create<string, string?>("Report:IntervalMinutes", intervalMinutes),
        ];

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
