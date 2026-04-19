using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ExchangeRateProvider.Domain.UnitTest.Markets;

public class TradingPairSnapshotTests
{
    [Fact]
    public void ToSnapshot_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var baseCurrency = new Currency("US Dollar", "USD", CurrencyType.Fiat, 1);
        var market = new Market(baseCurrency.Id, MarketCalculationTerm.Average, false, 1);

        market.BaseCurrency = baseCurrency;

        var currency = new Currency("Bitcoin", "BTC", CurrencyType.Crypto, 1, 8);
        var tradingPair = new MarketTradingPair(market.Id, currency.Id, 1, true, "BTC-USD");
        tradingPair.Market = market;
        tradingPair.Currency = currency;

        var providerIds = new List<int> { 1, 2 };

        // Act
        var snapshot = tradingPair.ToSnapshot(providerIds);

        // Assert
        snapshot.Id.Should().Be(0);
        snapshot.CurrencyId.Should().Be(tradingPair.CurrencyId);
        snapshot.CurrencyCode.Should().Be(currency.Code);
        snapshot.DecimalPrecision.Should().Be(currency.DecimalPrecision);
        snapshot.MarketId.Should().Be(market.Id);
        snapshot.BaseCurrencyCode.Should().Be(market.BaseCurrency.Code);
        snapshot.CalculationTerm.Should().Be(market.CalculationTerm);
        snapshot.SpreadOptions.Should().BeEquivalentTo(tradingPair.SpreadOptions);
        snapshot.ProviderIds.Should().BeEquivalentTo(providerIds);
    }

    [Fact]
    public void TradingPairSnapshot_Properties_AreInitializedCorrectly()
    {
        // Arrange
        var spreadOptions = new SpreadOptions { SpreadEnabled = true };
        var providerIds = new List<int> { 1, 2 };

        // Act
        var snapshot = new TradingPairSnapshot
        {
            Id = 1,
            CurrencyId = 10,
            CurrencyCode = "BTC",
            DecimalPrecision = 8,
            MarketId = 20,
            BaseCurrencyCode = "USD",
            CalculationTerm = MarketCalculationTerm.Average,
            SpreadOptions = spreadOptions,
            ProviderIds = providerIds
        };

        // Assert
        snapshot.Id.Should().Be(1);
        snapshot.CurrencyId.Should().Be(10);
        snapshot.CurrencyCode.Should().Be("BTC");
        snapshot.DecimalPrecision.Should().Be(8);
        snapshot.MarketId.Should().Be(20);
        snapshot.BaseCurrencyCode.Should().Be("USD");
        snapshot.CalculationTerm.Should().Be(MarketCalculationTerm.Average);
        snapshot.SpreadOptions.Should().Be(spreadOptions);
        snapshot.ProviderIds.Should().BeEquivalentTo(providerIds);
    }
}
