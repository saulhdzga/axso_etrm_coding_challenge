using Axso.PowerPositionReporter.Application;
using Axso.PowerPositionReporter.Application.Configuration;
using Axso.PowerPositionReporter.Application.Reports;
using Axso.PowerPositionReporter.Infrastructure;
using Axso.PowerPositionReporter.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class DependencyInjectionTests
{
    /// <summary>
    /// Verifies that application and infrastructure registrations resolve the report runner.
    /// </summary>
    [Fact]
    public void ServiceCollection_ResolvesReportRunner()
    {
        KeyValuePair<string, string?>[] configurationValues =
        [
            KeyValuePair.Create<string, string?>("Report:OutputPath", "reports"),
            KeyValuePair.Create<string, string?>("Report:IntervalMinutes", "15"),
            KeyValuePair.Create<string, string?>("PowerService:Retry:MaxRetryAttempts", "2"),
            KeyValuePair.Create<string, string?>("PowerService:Retry:DelayMilliseconds", "250"),
        ];

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        ServiceCollection services = new();
        services.AddLogging();
        services
            .AddPowerPositionApplication(configuration, [])
            .AddPowerPositionInfrastructure(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<IPowerPositionReportRunner>());
        ReportSettings settings = serviceProvider.GetRequiredService<ReportSettings>();
        Assert.Equal("reports", settings.OutputPath);
        Assert.Equal(15, settings.IntervalMinutes);
        PowerServiceRetrySettings retrySettings = serviceProvider.GetRequiredService<PowerServiceRetrySettings>();
        Assert.Equal(2, retrySettings.MaxRetryAttempts);
        Assert.Equal(250, retrySettings.DelayMilliseconds);
    }
}
