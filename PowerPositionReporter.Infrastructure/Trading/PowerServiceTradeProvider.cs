using Axpo;
using Axso.PowerPositionReporter.Application.Abstractions;
using Axso.PowerPositionReporter.Domain.Trades;
using Axso.PowerPositionReporter.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Axso.PowerPositionReporter.Infrastructure.Trading;

/// <summary>
/// Template boundary for retrieving power trades from the provided PowerService assembly.
/// </summary>
public sealed class PowerServiceTradeProvider : IPowerTradeProvider
{
    private readonly ILogger<PowerServiceTradeProvider> _logger;
    private readonly IPowerService _powerService;
    private readonly PowerServiceRetrySettings _retrySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PowerServiceTradeProvider"/> class.
    /// </summary>
    /// <param name="powerService">Provided service used to retrieve trades from the trading system.</param>
    /// <param name="retrySettings">Retry settings used for transient PowerService failures.</param>
    /// <param name="logger">Logger used to record retry attempts.</param>
    public PowerServiceTradeProvider(
        IPowerService powerService,
        PowerServiceRetrySettings retrySettings,
        ILogger<PowerServiceTradeProvider> logger)
    {
        _powerService = powerService;
        _retrySettings = retrySettings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PowerTradePosition>> GetTradesAsync(
        DateTime positionDate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<PowerTrade> trades = await GetTradesWithRetryAsync(positionDate, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return [.. trades.Select(ToDomainTrade)];
    }

    /// <summary>
    /// Retrieves trades from the PowerService with retry logic for transient failures.
    /// </summary>
    /// <param name="positionDate">Date for which trades should be retrieved from PowerService.</param>
    /// <param name="cancellationToken">Token used to cancel retry delays.</param>
    /// <returns>The trades returned by the PowerService assembly.</returns>
    private async Task<IEnumerable<PowerTrade>> GetTradesWithRetryAsync(
        DateTime positionDate,
        CancellationToken cancellationToken)
    {
        ResiliencePipeline<IEnumerable<PowerTrade>> retryPipeline = CreateRetryPipeline(positionDate);

        return await retryPipeline.ExecuteAsync(
            async currentCancellationToken =>
            {
                currentCancellationToken.ThrowIfCancellationRequested();

                return await _powerService.GetTradesAsync(positionDate);
            },
            cancellationToken);
    }

    /// <summary>
    /// Creates the Polly retry pipeline used for transient PowerService failures.
    /// </summary>
    /// <param name="positionDate">Date being retrieved, used for retry logging.</param>
    /// <returns>A retry pipeline for PowerService trade retrieval.</returns>
    private ResiliencePipeline<IEnumerable<PowerTrade>> CreateRetryPipeline(DateTime positionDate)
    {
        return new ResiliencePipelineBuilder<IEnumerable<PowerTrade>>()
            .AddRetry(new RetryStrategyOptions<IEnumerable<PowerTrade>>
            {
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromMilliseconds(_retrySettings.DelayMilliseconds),
                MaxRetryAttempts = _retrySettings.MaxRetryAttempts,
                ShouldHandle = new PredicateBuilder<IEnumerable<PowerTrade>>()
                    .Handle<PowerServiceException>(),
                OnRetry = retryArguments =>
                {
                    _logger.LogWarning(
                        retryArguments.Outcome.Exception,
                        "PowerService failed retrieving trades for {PositionDate}. Retrying attempt {NextAttempt} of {MaxAttempts}.",
                        positionDate,
                        retryArguments.AttemptNumber + 2,
                        _retrySettings.MaxRetryAttempts + 1);

                    return default;
                },
            })
            .Build();
    }

    /// <summary>
    /// Converts a PowerService trade to a domain trade position.
    /// </summary>
    /// <param name="trade">PowerService trade returned by the external assembly.</param>
    /// <returns>The domain representation used by the report pipeline.</returns>
    private static PowerTradePosition ToDomainTrade(PowerTrade trade)
    {
        PowerPeriodPosition[] periods =
        [
            .. trade.Periods.Select(period => new PowerPeriodPosition(
                Period: period.Period,
                Volume: (decimal)period.Volume))
        ];

        return new PowerTradePosition(trade.TradeId, trade.Date, periods);
    }
}
