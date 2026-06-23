using Axso.PowerPositionReporter.Application.Configuration;
using Axso.PowerPositionReporter.Application.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Axso.PowerPositionReporter.Application;

/// <summary>
/// Registers application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds power position reporter application services to the service collection.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="args">Command-line arguments supplied to the application.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPowerPositionApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        string[] args)
    {
        ReportSettings settings = ReportSettings.Load(configuration, args);

        services.AddSingleton(settings);
        services.AddSingleton<IPowerPositionAggregator, PowerPositionAggregator>();
        services.AddSingleton<IPowerPositionReportRunner, PowerPositionReportRunner>();
        services.AddSingleton<PowerPositionScheduler>();
        services.AddHostedService<PowerPositionSchedulerHostedService>();

        return services;
    }
}
