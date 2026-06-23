using Microsoft.Extensions.Configuration;

namespace Axso.PowerPositionReporter.Application.Configuration;

/// <summary>
/// Holds runtime settings used by the power position report scaffold.
/// </summary>
/// <param name="OutputPath">Directory path where generated report files should be written.</param>
/// <param name="IntervalMinutes">Scheduled interval, in minutes, between report extracts.</param>
public sealed record ReportSettings(string OutputPath, int IntervalMinutes)
{
    /// <summary>
    /// Creates report settings from application configuration and optional command-line overrides.
    /// </summary>
    /// <param name="configuration">Application configuration containing the Report section.</param>
    /// <param name="args">Command-line arguments supplied to the application.</param>
    /// <returns>A <see cref="ReportSettings"/> instance populated from configuration and overrides.</returns>
    public static ReportSettings Load(IConfiguration configuration, string[] args)
    {
        string outputPath = args.Length > 0
            ? args[0]
            : GetRequiredString(configuration, "Report:OutputPath");
        int intervalMinutes = args.Length > 1
            ? GetRequiredInt(args[1], "command-line interval")
            : GetRequiredInt(configuration, "Report:IntervalMinutes");

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("Report output path is required.");
        }

        if (intervalMinutes <= 0)
        {
            throw new InvalidOperationException("Report interval must be greater than zero minutes.");
        }

        return new ReportSettings(outputPath, intervalMinutes);
    }

    /// <summary>
    /// Reads a required string value from configuration.
    /// </summary>
    /// <param name="configuration">Configuration to read from.</param>
    /// <param name="key">Configuration key to read.</param>
    /// <returns>The configured string value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is missing or whitespace.</exception>
    private static string GetRequiredString(IConfiguration configuration, string key)
    {
        string? value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{key} is required.");
        }

        return value;
    }

    /// <summary>
    /// Reads a required integer value from configuration.
    /// </summary>
    /// <param name="configuration">Configuration to read from.</param>
    /// <param name="key">Configuration key to read.</param>
    /// <returns>The configured integer value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is missing or invalid.</exception>
    private static int GetRequiredInt(IConfiguration configuration, string key)
    {
        return GetRequiredInt(configuration[key], key);
    }

    /// <summary>
    /// Parses a required integer value.
    /// </summary>
    /// <param name="value">Value to parse.</param>
    /// <param name="settingName">Setting name used in error messages.</param>
    /// <returns>The parsed integer value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is missing or invalid.</exception>
    private static int GetRequiredInt(string? value, string settingName)
    {
        if (!int.TryParse(value, out int parsedValue))
        {
            throw new InvalidOperationException($"{settingName} must be a valid integer.");
        }

        return parsedValue;
    }
}
