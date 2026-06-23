using Axso.PowerPositionReporter.Application;
using Axso.PowerPositionReporter.Application.Configuration;
using Axso.PowerPositionReporter.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

using IHost host = Host
    .CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services);
    })
    .ConfigureServices((context, services) =>
    {
        services
            .AddPowerPositionApplication(context.Configuration, args)
            .AddPowerPositionInfrastructure(context.Configuration);
    })
    .Build();

try
{
    ReportSettings resolvedSettings = host.Services.GetRequiredService<ReportSettings>();

    Console.WriteLine("Power Position Reporter");
    Console.WriteLine($"Output path: {resolvedSettings.OutputPath}");
    Console.WriteLine($"Interval minutes: {resolvedSettings.IntervalMinutes}");
    Console.WriteLine("Starting scheduler. Press Ctrl+C to stop.");

    await host.RunAsync();
}
catch (Exception exception)
{
    ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
    Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger("PowerPositionReporter");
    logger.LogCritical(exception, "Application terminated unexpectedly.");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
