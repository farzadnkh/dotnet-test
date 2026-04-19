using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.ExchangeRateProviders;

public class CurrencyExchangeRateTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeAllPropertiesCorrectly()
    {
        // Arrange
        const long consumerId = 101;
        const long marketTradingPairId = 202;
        const decimal originalRate = 1.25m;
        const decimal buy = 1.26m;
        const RateChangeType buyRateChange = RateChangeType.Increased;
        const decimal sell = 1.24m;
        const RateChangeType sellRateChange = RateChangeType.Decreased;
        var createdOnUtc = new DateTimeOffset(2025, 9, 1, 10, 0, 0, TimeSpan.Zero);

        // Act
        var exchangeRate = new CurrencyExchangeRate(
            consumerId,
            marketTradingPairId,
            originalRate,
            buy,
            buyRateChange,
            sell,
            sellRateChange,
            createdOnUtc);

        // Assert
        exchangeRate.ConsumerId.Should().Be(consumerId);
        exchangeRate.MarketTradingPairId.Should().Be(marketTradingPairId);
        exchangeRate.OriginalRate.Should().Be(originalRate);
        exchangeRate.Buy.Should().Be(buy);
        exchangeRate.BuyRateChange.Should().Be(buyRateChange);
        exchangeRate.Sell.Should().Be(sell);
        exchangeRate.SellRateChange.Should().Be(sellRateChange);
        exchangeRate.CreatedOnUtc.Should().Be(createdOnUtc);
        exchangeRate.UpdatedOnUtc.Should().BeNull();
    }
    [Fact]
    public void UpdateBuyRate_WithNewValues_ShouldUpdateBuyProperties()
    {
        // Arrange
        var exchangeRate = CreateTestExchangeRate();
        const decimal newBuyRate = 1.30m;
        const RateChangeType newChangeType = RateChangeType.Increased;

        // Act
        exchangeRate.UpdateBuyRate(newBuyRate, newChangeType);

        // Assert
        exchangeRate.Buy.Should().Be(newBuyRate);
        exchangeRate.BuyRateChange.Should().Be(newChangeType);
        exchangeRate.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        exchangeRate.OriginalRate.Should().Be(1.25m);
        exchangeRate.Sell.Should().Be(1.24m);
    }


    [Fact]
    public void UpdateSellRate_WithNewValues_ShouldUpdateSellProperties()
    {
        // Arrange
        var exchangeRate = CreateTestExchangeRate();
        const decimal newSellRate = 1.20m;
        const RateChangeType newChangeType = RateChangeType.Decreased;

        // Act
        exchangeRate.UpdateSellRate(newSellRate, newChangeType);

        // Assert
        exchangeRate.Sell.Should().Be(newSellRate);
        exchangeRate.SellRateChange.Should().Be(newChangeType);
        exchangeRate.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        exchangeRate.OriginalRate.Should().Be(1.25m);
        exchangeRate.Buy.Should().Be(1.26m);
    }

    [Fact]
    public void SetOriginalRate_WithNewRate_ShouldUpdateOriginalRate()
    {
        // Arrange
        var exchangeRate = CreateTestExchangeRate();
        const decimal newOriginalRate = 1.50m;

        // Act
        exchangeRate.SetOriginalRate(newOriginalRate);

        // Assert
        exchangeRate.OriginalRate.Should().Be(newOriginalRate);
        exchangeRate.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        exchangeRate.Buy.Should().Be(1.26m);
        exchangeRate.Sell.Should().Be(1.24m);
    }

    private CurrencyExchangeRate CreateTestExchangeRate()
    {
        return new CurrencyExchangeRate(
            consumerId: 101,
            marketTradingPairId: 202,
            originalRate: 1.25m,
            buy: 1.26m,
            buyRateChange: RateChangeType.Increased,
            sell: 1.24m,
            sellRateChange: RateChangeType.Decreased,
            createdOnUtc: DateTimeOffset.UtcNow
        );
    }
}