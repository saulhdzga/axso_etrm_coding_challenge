using Axso.PowerPositionReporter.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class PowerServiceRetrySettingsTests
{
    /// <summary>
    /// Verifies that PowerService retry settings are read from configuration.
    /// </summary>
    [Fact]
    public void Load_UsesConfigurationValues()
    {
        IConfiguration configuration = CreateConfiguration(
            maxRetryAttempts: "4",
            delayMilliseconds: "100");

        PowerServiceRetrySettings settings = PowerServiceRetrySettings.Load(configuration);

        Assert.Equal(4, settings.MaxRetryAttempts);
        Assert.Equal(100, settings.DelayMilliseconds);
    }

    /// <summary>
    /// Verifies that negative retry attempts are rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsNegativeRetryAttempts()
    {
        IConfiguration configuration = CreateConfiguration(
            maxRetryAttempts: "-1",
            delayMilliseconds: "100");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => PowerServiceRetrySettings.Load(configuration));

        Assert.Equal("PowerService retry attempts cannot be negative.", exception.Message);
    }

    /// <summary>
    /// Verifies that negative retry delays are rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsNegativeRetryDelay()
    {
        IConfiguration configuration = CreateConfiguration(
            maxRetryAttempts: "2",
            delayMilliseconds: "-1");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => PowerServiceRetrySettings.Load(configuration));

        Assert.Equal("PowerService retry delay cannot be negative.", exception.Message);
    }

    /// <summary>
    /// Verifies that missing retry attempts are rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsMissingRetryAttempts()
    {
        IConfiguration configuration = CreateConfiguration(
            maxRetryAttempts: "",
            delayMilliseconds: "100");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => PowerServiceRetrySettings.Load(configuration));

        Assert.Equal("PowerService:Retry:MaxRetryAttempts must be a valid integer.", exception.Message);
    }

    /// <summary>
    /// Verifies that missing retry delay is rejected.
    /// </summary>
    [Fact]
    public void Load_RejectsMissingRetryDelay()
    {
        IConfiguration configuration = CreateConfiguration(
            maxRetryAttempts: "2",
            delayMilliseconds: "");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => PowerServiceRetrySettings.Load(configuration));

        Assert.Equal("PowerService:Retry:DelayMilliseconds must be a valid integer.", exception.Message);
    }

    /// <summary>
    /// Creates an in-memory configuration for PowerService retry settings tests.
    /// </summary>
    /// <param name="maxRetryAttempts">Configured maximum retry attempts.</param>
    /// <param name="delayMilliseconds">Configured delay in milliseconds.</param>
    /// <returns>The in-memory configuration.</returns>
    private static IConfiguration CreateConfiguration(string maxRetryAttempts, string delayMilliseconds)
    {
        KeyValuePair<string, string?>[] values =
        [
            KeyValuePair.Create<string, string?>("PowerService:Retry:MaxRetryAttempts", maxRetryAttempts),
            KeyValuePair.Create<string, string?>("PowerService:Retry:DelayMilliseconds", delayMilliseconds),
        ];

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
