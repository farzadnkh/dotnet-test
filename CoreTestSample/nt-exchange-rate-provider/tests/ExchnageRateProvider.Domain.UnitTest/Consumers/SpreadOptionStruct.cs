using ExchangeRateProvider.Application.Consumers.Extensions;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.Consumers;

public class SpreadOptionStructTests
{
    [Fact]
    public void GetActiveSpreadOption_WhenConsumerPairHasEnabledSpread_ShouldReturnConsumerPairSpreadOptions()
    {
        // Arrange
        var consumerPairSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 1, UpperLimitPercentage = 2 };
        var consumerMarketSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 3, UpperLimitPercentage = 4 };
        var tradingPairSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 5, UpperLimitPercentage = 6 };
        var marketSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 7, UpperLimitPercentage = 8 };

        var consumerPair = new ConsumerPair(1, 1, 1, true, 1);
        consumerPair.SetSpreadLimits(consumerPairSpread);

        var consumerMarket = new ConsumerMarket(1, 1, true, 1, DateTimeOffset.UtcNow);
        consumerMarket.SetSpreadLimits(consumerMarketSpread);

        var tradingPair = new TradingPairSnapshot { SpreadOptions = tradingPairSpread };
        var market = new Market(1, MarketCalculationTerm.Average, true, 1);
        market.EnableSpread(7, 8);

        var spreadStruct = new SpreadOptionStruct(consumerPair, consumerMarket, tradingPair, market);

        // Act
        var result = spreadStruct.GetActiveSpreadOption();

        // Assert
        result.LowerLimitPercentage.Should().Be(consumerPairSpread.LowerLimitPercentage);
        result.UpperLimitPercentage.Should().Be(consumerPairSpread.UpperLimitPercentage);
        result.SpreadEnabled.Should().Be(consumerPairSpread.SpreadEnabled);
    }

    [Fact]
    public void GetActiveSpreadOption_WhenConsumerPairDisabled_ShouldReturnConsumerMarketSpreadOptions()
    {
        // Arrange
        var consumerPairSpread = new SpreadOptions { SpreadEnabled = false };
        var consumerMarketSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 3, UpperLimitPercentage = 4 };
        var tradingPairSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 5, UpperLimitPercentage = 6 };
        var marketSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 7, UpperLimitPercentage = 8 };

        var consumerPair = new ConsumerPair(1, 1, 1, true, 1);
        consumerPair.SetSpreadLimits(consumerPairSpread);

        var consumerMarket = new ConsumerMarket(1, 1, true, 1, DateTimeOffset.UtcNow);
        consumerMarket.SetSpreadLimits(consumerMarketSpread);

        var tradingPair = new TradingPairSnapshot { SpreadOptions = tradingPairSpread };
        var market = new Market(1, MarketCalculationTerm.Average, true, 1);
        market.EnableSpread(7, 8);

        var spreadStruct = new SpreadOptionStruct(consumerPair, consumerMarket, tradingPair, market);

        // Act
        var result = spreadStruct.GetActiveSpreadOption();

        // Assert
        result.LowerLimitPercentage.Should().Be(consumerMarketSpread.LowerLimitPercentage);
        result.UpperLimitPercentage.Should().Be(consumerMarketSpread.UpperLimitPercentage);
        result.SpreadEnabled.Should().Be(consumerMarketSpread.SpreadEnabled);
    }

    [Fact]
    public void GetActiveSpreadOption_WhenConsumerAndConsumerMarketDisabled_ShouldReturnTradingPairSpreadOptions()
    {
        // Arrange
        var consumerPairSpread = new SpreadOptions { SpreadEnabled = false };
        var consumerMarketSpread = new SpreadOptions { SpreadEnabled = false };
        var tradingPairSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 5, UpperLimitPercentage = 6 };
        var marketSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 7, UpperLimitPercentage = 8 };

        var consumerPair = new ConsumerPair(1, 1, 1, true, 1);
        consumerPair.SetSpreadLimits(consumerPairSpread);

        var consumerMarket = new ConsumerMarket(1, 1, true, 1, DateTimeOffset.UtcNow);
        consumerMarket.SetSpreadLimits(consumerMarketSpread);

        var tradingPair = new TradingPairSnapshot { SpreadOptions = tradingPairSpread };
        var market = new Market(1, MarketCalculationTerm.Average, true, 1);
        market.EnableSpread(7, 8);

        var spreadStruct = new SpreadOptionStruct(consumerPair, consumerMarket, tradingPair, market);

        // Act
        var result = spreadStruct.GetActiveSpreadOption();

        // Assert
        result.Should().BeEquivalentTo(tradingPairSpread);
    }

    [Fact]
    public void GetActiveSpreadOption_WhenTradingPairDisabled_ShouldReturnMarketSpreadOptions()
    {
        // Arrange
        var consumerPairSpread = new SpreadOptions { SpreadEnabled = false };
        var consumerMarketSpread = new SpreadOptions { SpreadEnabled = false };
        var tradingPairSpread = new SpreadOptions { SpreadEnabled = false };
        var marketSpread = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 7, UpperLimitPercentage = 8 };

        var consumerPair = new ConsumerPair(1, 1, 1, true, 1);
        consumerPair.SetSpreadLimits(consumerPairSpread);

        var consumerMarket = new ConsumerMarket(1, 1, true, 1, DateTimeOffset.UtcNow);
        consumerMarket.SetSpreadLimits(consumerMarketSpread);

        var tradingPair = new TradingPairSnapshot { SpreadOptions = tradingPairSpread };
        var market = new Market(1, MarketCalculationTerm.Average, true, 1);
        market.EnableSpread(7, 8);

        var spreadStruct = new SpreadOptionStruct(consumerPair, consumerMarket, tradingPair, market);

        // Act
        var result = spreadStruct.GetActiveSpreadOption();

        // Assert
        result.Should().BeSameAs(market.SpreadOptions);
    }

    [Fact]
    public void GetActiveSpreadOption_WhenNoSpreadEnabled_ShouldReturnNull()
    {
        // Arrange
        var consumerPairSpread = new SpreadOptions { SpreadEnabled = false };
        var consumerMarketSpread = new SpreadOptions { SpreadEnabled = false };
        var tradingPairSpread = new SpreadOptions { SpreadEnabled = false };
        var marketSpread = new SpreadOptions { SpreadEnabled = false };

        var consumerPair = new ConsumerPair(1, 1, 1, true, 1);
        consumerPair.SetSpreadLimits(consumerPairSpread);

        var consumerMarket = new ConsumerMarket(1, 1, true, 1, DateTimeOffset.UtcNow);
        consumerMarket.SetSpreadLimits(consumerMarketSpread);

        var tradingPair = new TradingPairSnapshot { SpreadOptions = tradingPairSpread };
        var market = new Market(1, MarketCalculationTerm.Average, false, 1);
        market.Update(1, MarketCalculationTerm.Average, marketSpread);

        var spreadStruct = new SpreadOptionStruct(consumerPair, consumerMarket, tradingPair, market);

        // Act
        var result = spreadStruct.GetActiveSpreadOption();

        // Assert
        result.Should().BeNull();
    }
}