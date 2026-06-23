using Axpo;
using Axso.PowerPositionReporter.Domain.Trades;

namespace Axso.PowerPositionReporter.Tests.TestData;

internal static class TestPowerTrades
{
    /// <summary>
    /// Creates a domain trade from period volumes.
    /// </summary>
    /// <param name="date">Position date for the trade.</param>
    /// <param name="volumes">Period volumes to include in the trade.</param>
    /// <param name="tradeId">Identifier to assign to the trade.</param>
    /// <returns>A domain trade with one period per supplied volume.</returns>
    public static PowerTradePosition CreateDomainTrade(
        DateTime date,
        IReadOnlyList<decimal> volumes,
        string tradeId = "trade-1")
    {
        PowerPeriodPosition[] periods =
        [
            .. volumes.Select((volume, index) => new PowerPeriodPosition(
                Period: index + 1,
                Volume: volume))
        ];

        return new PowerTradePosition(tradeId, date, periods);
    }

    /// <summary>
    /// Creates a PowerService trade from period volumes.
    /// </summary>
    /// <param name="date">Position date for the trade.</param>
    /// <param name="volumes">Period volumes to set on the PowerService trade.</param>
    /// <returns>A PowerService trade with one period per supplied volume.</returns>
    public static PowerTrade Create(DateTime date, IReadOnlyList<double> volumes)
    {
        PowerTrade trade = PowerTrade.Create(date, volumes.Count);

        for (int index = 0; index < volumes.Count; index++)
        {
            trade.Periods[index].SetVolume(volumes[index]);
        }

        return trade;
    }
}
