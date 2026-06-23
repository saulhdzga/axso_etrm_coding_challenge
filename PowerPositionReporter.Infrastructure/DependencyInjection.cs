using Axpo;
using Axso.PowerPositionReporter.Application.Abstractions;
using Axso.PowerPositionReporter.Infrastructure.Configuration;
using Axso.PowerPositionReporter.Infrastructure.Csv;
using Axso.PowerPositionReporter.Infrastructure.Time;
using Axso.PowerPositionReporter.Infrastructure.Trading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Axso.PowerPositionReporter.Infrastructure;

/// <summary>
/// Registers infrastructure-layer adapters.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds power position reporter infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPowerPositionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        PowerServiceRetrySettings retrySettings = PowerServiceRetrySettings.Load(configuration);

        services.AddSingleton(retrySettings);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IDelayProvider, SystemDelayProvider>();
        services.AddSingleton<IPowerService, PowerService>();
        services.AddSingleton<IPowerTradeProvider, PowerServiceTradeProvider>();
        services.AddSingleton<IPowerTradeCsvWriter, PowerTradeCsvWriter>();
        services.AddSingleton<IPowerPositionCsvWriter, PowerPositionCsvWriter>();

        return services;
    }
}
