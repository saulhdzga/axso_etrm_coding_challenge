using Axso.PowerPositionReporter.Application.Configuration;
using Axso.PowerPositionReporter.Domain.Positions;
using Axso.PowerPositionReporter.Domain.Trades;
using Axso.PowerPositionReporter.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Axso.PowerPositionReporter.Application.Reports;

/// <summary>
/// Coordinates the steps required to produce a single power position report extract.
/// </summary>
public sealed class PowerPositionReportRunner : IPowerPositionReportRunner
{
    private readonly IPowerPositionAggregator _aggregator;
    private readonly IClock _clock;
    private readonly ILogger<PowerPositionReportRunner> _logger;
    private readonly IPowerTradeCsvWriter _tradeWriter;
    private readonly IPowerTradeProvider _tradeProvider;
    private readonly IPowerPositionCsvWriter _writer;
    private readonly ReportSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PowerPositionReportRunner"/> class.
    /// </summary>
    /// <param name="aggregator">Aggregates raw power trades into hourly positions.</param>
    /// <param name="tradeProvider">Retrieves raw power trades.</param>
    /// <param name="tradeWriter">Writes raw power trades for diagnostic verification.</param>
    /// <param name="writer">Writes the aggregated positions to CSV.</param>
    /// <param name="clock">Provides the extract time.</param>
    /// <param name="settings">Runtime report settings.</param>
    /// <param name="logger">Logger used to record extract progress and failures.</param>
    public PowerPositionReportRunner(
        IPowerPositionAggregator aggregator,
        IPowerTradeProvider tradeProvider,
        IPowerTradeCsvWriter tradeWriter,
        IPowerPositionCsvWriter writer,
        IClock clock,
        ReportSettings settings,
        ILogger<PowerPositionReportRunner> logger)
    {
        _aggregator = aggregator;
        _tradeProvider = tradeProvider;
        _tradeWriter = tradeWriter;
        _writer = writer;
        _clock = clock;
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        DateTime extractLocalTime = _clock.GetLocalNow();
        DateTime positionDate = extractLocalTime.Date;

        _logger.LogInformation(
            "Starting power position extract for {PositionDate} at {ExtractLocalTime}.",
            positionDate,
            extractLocalTime);

        try
        {
            IReadOnlyCollection<PowerTradePosition> trades = await _tradeProvider.GetTradesAsync(
                positionDate,
                cancellationToken);

            string rawTradesFilePath = await _tradeWriter.WriteAsync(
                _settings.OutputPath,
                extractLocalTime,
                trades,
                cancellationToken);

            _logger.LogInformation(
                "Wrote raw power trades diagnostic file to {RawTradesFilePath}. Trades: {TradeCount}.",
                rawTradesFilePath,
                trades.Count);

            IReadOnlyCollection<HourlyPowerPosition> positions = _aggregator.Aggregate(trades);

            await _writer.WriteAsync(
                _settings.OutputPath,
                extractLocalTime,
                positions,
                cancellationToken);

            _logger.LogInformation(
                "Completed power position extract for {PositionDate}. Rows written: {RowCount}.",
                positionDate,
                positions.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Power position extract was cancelled.");
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Power position extract failed for {PositionDate}.", positionDate);
            throw;
        }
    }
}
