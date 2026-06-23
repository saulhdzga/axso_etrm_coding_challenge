using Microsoft.Extensions.Configuration;

namespace Axso.PowerPositionReporter.Infrastructure.Configuration;

/// <summary>
/// Holds retry settings for calls to the provided PowerService assembly.
/// </summary>
/// <param name="MaxRetryAttempts">Maximum number of retry attempts after the initial call.</param>
/// <param name="DelayMilliseconds">Delay, in milliseconds, between retry attempts.</param>
public sealed record PowerServiceRetrySettings(int MaxRetryAttempts, int DelayMilliseconds)
{
    /// <summary>
    /// Creates retry settings from application configuration.
    /// </summary>
    /// <param name="configuration">Application configuration containing the PowerService retry section.</param>
    /// <returns>A <see cref="PowerServiceRetrySettings"/> instance populated from configuration.</returns>
    public static PowerServiceRetrySettings Load(IConfiguration configuration)
    {
        int maxRetryAttempts = GetRequiredInt(
            configuration,
            "PowerService:Retry:MaxRetryAttempts");
        int delayMilliseconds = GetRequiredInt(
            configuration,
            "PowerService:Retry:DelayMilliseconds");

        if (maxRetryAttempts < 0)
        {
            throw new InvalidOperationException("PowerService retry attempts cannot be negative.");
        }

        if (delayMilliseconds < 0)
        {
            throw new InvalidOperationException("PowerService retry delay cannot be negative.");
        }

        return new PowerServiceRetrySettings(maxRetryAttempts, delayMilliseconds);
    }

    /// <summary>
    /// Reads a required integer configuration value.
    /// </summary>
    /// <param name="configuration">Configuration to read from.</param>
    /// <param name="key">Configuration key to read.</param>
    /// <returns>The configured integer value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is missing or invalid.</exception>
    private static int GetRequiredInt(IConfiguration configuration, string key)
    {
        if (!int.TryParse(configuration[key], out int configuredValue))
        {
            throw new InvalidOperationException($"{key} must be a valid integer.");
        }

        return configuredValue;
    }
}
