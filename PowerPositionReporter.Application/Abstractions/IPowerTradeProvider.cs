using Axso.PowerPositionReporter.Domain.Trades;

namespace Axso.PowerPositionReporter.Application.Abstractions;

/// <summary>
/// Defines the boundary for retrieving raw power trades from the trading system.
/// </summary>
public interface IPowerTradeProvider
{
    /// <summary>
    /// Retrieves power trades for the requested position date.
    /// </summary>
    /// <param name="positionDate">Date for which trades should be retrieved.</param>
    /// <param name="cancellationToken">Token used to cancel the retrieval operation.</param>
    /// <returns>Power trades returned by the trading system.</returns>
    Task<IReadOnlyCollection<PowerTradePosition>> GetTradesAsync(DateTime positionDate, CancellationToken cancellationToken);
}
