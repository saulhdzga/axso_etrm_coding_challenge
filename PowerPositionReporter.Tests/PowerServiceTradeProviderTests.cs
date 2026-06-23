using Axpo;
using Axso.PowerPositionReporter.Domain.Trades;
using Axso.PowerPositionReporter.Infrastructure.Configuration;
using Axso.PowerPositionReporter.Infrastructure.Trading;
using Axso.PowerPositionReporter.Tests.TestData;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Axso.PowerPositionReporter.Tests;

public sealed class PowerServiceTradeProviderTests
{
    /// <summary>
    /// Verifies that trades returned by PowerService are converted to domain trades.
    /// </summary>
    [Fact]
    public async Task GetTradesAsync_ReturnsTradesFromPowerService()
    {
        DateTime positionDate = new(2026, 06, 21);
        PowerTrade trade = TestPowerTrades.Create(positionDate, [.. Enumerable.Repeat(10d, 24)]);
        Mock<IPowerService> powerService = new();
        powerService
            .Setup(service => service.GetTradesAsync(positionDate))
            .ReturnsAsync([trade]);

        PowerServiceTradeProvider provider = new(
            powerService.Object,
            CreateRetrySettings(),
            NullLogger<PowerServiceTradeProvider>.Instance);

        IReadOnlyCollection<PowerTradePosition> trades = await provider.GetTradesAsync(positionDate, CancellationToken.None);

        PowerTradePosition returnedTrade = Assert.Single(trades);
        Assert.Equal(trade.TradeId, returnedTrade.TradeId);
        Assert.Equal(trade.Date, returnedTrade.Date);
        Assert.Equal(24, returnedTrade.Periods.Count);
        Assert.Equal(10m, returnedTrade.Periods.First().Volume);
        powerService.Verify(service => service.GetTradesAsync(positionDate), Times.Once);
    }

    /// <summary>
    /// Verifies that transient PowerService exceptions are retried.
    /// </summary>
    [Fact]
    public async Task GetTradesAsync_RetriesPowerServiceExceptions()
    {
        DateTime positionDate = new(2026, 06, 21);
        PowerTrade trade = TestPowerTrades.Create(positionDate, [.. Enumerable.Repeat(10d, 24)]);
        Mock<IPowerService> powerService = new();
        powerService
            .SetupSequence(service => service.GetTradesAsync(positionDate))
            .ThrowsAsync(new PowerServiceException("Transient service failure."))
            .ReturnsAsync([trade]);

        PowerServiceTradeProvider provider = new(
            powerService.Object,
            CreateRetrySettings(),
            NullLogger<PowerServiceTradeProvider>.Instance);

        IReadOnlyCollection<PowerTradePosition> trades = await provider.GetTradesAsync(positionDate, CancellationToken.None);

        Assert.Single(trades);
        powerService.Verify(service => service.GetTradesAsync(positionDate), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that PowerService failures are retried up to the configured limit.
    /// </summary>
    [Fact]
    public async Task GetTradesAsync_ThrowsAfterRetryLimitIsReached()
    {
        DateTime positionDate = new(2026, 06, 21);
        PowerServiceException expectedException = new("Persistent service failure.");
        Mock<IPowerService> powerService = new();
        powerService
            .Setup(service => service.GetTradesAsync(positionDate))
            .ThrowsAsync(expectedException);

        PowerServiceTradeProvider provider = new(
            powerService.Object,
            CreateRetrySettings(),
            NullLogger<PowerServiceTradeProvider>.Instance);

        PowerServiceException actualException = await Assert.ThrowsAsync<PowerServiceException>(
            () => provider.GetTradesAsync(positionDate, CancellationToken.None));

        Assert.Same(expectedException, actualException);
        powerService.Verify(service => service.GetTradesAsync(positionDate), Times.Exactly(3));
    }

    /// <summary>
    /// Verifies that the configured retry count controls the number of PowerService calls.
    /// </summary>
    [Fact]
    public async Task GetTradesAsync_UsesConfiguredRetryAttempts()
    {
        DateTime positionDate = new(2026, 06, 21);
        PowerServiceException expectedException = new("Persistent service failure.");
        Mock<IPowerService> powerService = new();
        powerService
            .Setup(service => service.GetTradesAsync(positionDate))
            .ThrowsAsync(expectedException);

        PowerServiceTradeProvider provider = new(
            powerService.Object,
            new PowerServiceRetrySettings(MaxRetryAttempts: 4, DelayMilliseconds: 0),
            NullLogger<PowerServiceTradeProvider>.Instance);

        PowerServiceException actualException = await Assert.ThrowsAsync<PowerServiceException>(
            () => provider.GetTradesAsync(positionDate, CancellationToken.None));

        Assert.Same(expectedException, actualException);
        powerService.Verify(service => service.GetTradesAsync(positionDate), Times.Exactly(5));
    }

    /// <summary>
    /// Creates default retry settings for PowerService provider tests.
    /// </summary>
    /// <returns>Retry settings matching application defaults.</returns>
    private static PowerServiceRetrySettings CreateRetrySettings()
    {
        return new PowerServiceRetrySettings(MaxRetryAttempts: 2, DelayMilliseconds: 250);
    }
}
