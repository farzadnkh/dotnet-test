using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Consumers.Entities;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.Consumers;

public class ConsumerTests
{
    [Fact]
    public void AddMarket_WithValidSpreadOptions_ShouldAddConsumerMarketWithCorrectSpread()
    {
        // Arrange
        var consumer = CreateTestConsumer();
        var spreadOptions = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 1, UpperLimitPercentage = 5 };

        // Act
        consumer.AddMarket(1, 1, true, spreadOptions);

        // Assert
        consumer.ConsumerMarkets.Should().ContainSingle();
        var addedMarket = consumer.ConsumerMarkets.First();
        addedMarket.SpreadOptions.Should().BeEquivalentTo(spreadOptions);
    }

    [Fact]
    public void AddTradingPairs_WithValidSpreadOptions_ShouldAddConsumerPairWithCorrectSpread()
    {
        // Arrange
        var consumer = CreateTestConsumer();
        var spreadOptions = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 2, UpperLimitPercentage = 8 };

        // Act
        consumer.AddTradingPairs(1, 1, 1, true, spreadOptions);

        // Assert
        consumer.ConsumerPairs.Should().ContainSingle();
        var addedPair = consumer.ConsumerPairs.First();
        addedPair.SpreadOptions.Should().BeEquivalentTo(spreadOptions);
    }

    [Fact]
    public void ClearAssociations_ShouldRemoveAllConsumerMarketsPairsAndProviders()
    {
        // Arrange
        var consumer = CreateTestConsumer();
        consumer.AddMarket(1, 1, true, new SpreadOptions());
        consumer.AddTradingPairs(1, 1, 1, true, new SpreadOptions());
        consumer.AddProvider(1, true, 1);

        consumer.ConsumerMarkets.Should().NotBeEmpty();
        consumer.ConsumerPairs.Should().NotBeEmpty();
        consumer.ConsumerProviders.Should().NotBeEmpty();

        // Act
        consumer.ClearAssociations();

        // Assert
        consumer.ConsumerMarkets.Should().BeEmpty();
        consumer.ConsumerPairs.Should().BeEmpty();
        consumer.ConsumerProviders.Should().BeEmpty();
    }

    [Fact]
    public void GetSpreadPrice_WithValidOptions_ShouldReturnCorrectPrices()
    {
        // Arrange
        var spreadOptions = new SpreadOptions
        {
            SpreadEnabled = true,
            LowerLimitPercentage = 5,
            UpperLimitPercentage = 10
        };
        const decimal price = 100m;

        // Act
        var (upperLimit, lowerLimit) = spreadOptions.GetSpreadPrice(price);

        // Assert
        upperLimit.Should().Be(110m);
        lowerLimit.Should().Be(95m);
    }

    [Fact]
    public void GetSpreadPrice_WithZeroPercentages_ShouldReturnOriginalPrice()
    {
        // Arrange
        var spreadOptions = new SpreadOptions
        {
            SpreadEnabled = true,
            LowerLimitPercentage = 0,
            UpperLimitPercentage = 0
        };
        const decimal price = 100m;

        // Act
        var (upperLimit, lowerLimit) = spreadOptions.GetSpreadPrice(price);

        // Assert
        upperLimit.Should().Be(100m);
        lowerLimit.Should().Be(100m);
    }

    private Consumer CreateTestConsumer()
    {
        return new Consumer(
            userId: 1,
            projectName: "Test Project",
            createdById: 1
        );
    }
}
