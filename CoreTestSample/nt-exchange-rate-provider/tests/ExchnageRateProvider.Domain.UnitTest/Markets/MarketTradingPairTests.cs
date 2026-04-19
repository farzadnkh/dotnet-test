using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Markets.Entities;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.Markets;

public class MarketTradingPairTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        const int marketId = 1;
        const int currencyId = 2;
        const int creatorId = 3;
        const bool published = true;
        const string description = "EUR-USD";

        // Act
        var tradingPair = MarketTradingPair.Create(marketId, currencyId, creatorId, published, description);

        // Assert
        tradingPair.MarketId.Should().Be(marketId);
        tradingPair.CurrencyId.Should().Be(currencyId);
        tradingPair.CreatedById.Should().Be(creatorId);
        tradingPair.Published.Should().Be(published);
        tradingPair.Description.Should().Be(description);
        tradingPair.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        tradingPair.SpreadOptions.SpreadEnabled.Should().BeFalse();
    }

    [Fact]
    public void EnableSpread_WithValidLimits_ShouldSetSpreadOptions()
    {
        // Arrange
        var tradingPair = CreateTestTradingPair();
        const decimal lower = 0.1m;
        const decimal upper = 0.5m;

        // Act
        tradingPair.EnableSpread(lower, upper);

        // Assert
        tradingPair.SpreadOptions.SpreadEnabled.Should().BeTrue();
        tradingPair.SpreadOptions.LowerLimitPercentage.Should().Be(lower);
        tradingPair.SpreadOptions.UpperLimitPercentage.Should().Be(upper);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    [InlineData(10, 5)]
    public void EnableSpread_WithInvalidLimits_ShouldThrowInvalidOperationException(decimal lower, decimal upper)
    {
        // Arrange
        var tradingPair = CreateTestTradingPair();

        // Act
        Action act = () => tradingPair.EnableSpread(lower, upper);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid spread limits.");
    }

    [Fact]
    public void Update_WithNewValues_ShouldChangeProperties()
    {
        // Arrange
        var tradingPair = CreateTestTradingPair();
        const int newMarketId = 2;
        const int newCurrencyId = 3;
        var newSpreadOptions = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 1, UpperLimitPercentage = 2 };
        const string newDescription = "New Description";
        const bool newIsPublished = false;

        // Act
        tradingPair.Update(newMarketId, newCurrencyId, newSpreadOptions, newDescription, newIsPublished);

        // Assert
        tradingPair.MarketId.Should().Be(newMarketId);
        tradingPair.CurrencyId.Should().Be(newCurrencyId);
        tradingPair.SpreadOptions.Should().BeEquivalentTo(newSpreadOptions);
        tradingPair.Description.Should().Be(newDescription);
        tradingPair.Published.Should().Be(newIsPublished);
        tradingPair.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetExchangeRateProviders_ShouldAddUniqueProvidersToCollection()
    {
        // Arrange
        var tradingPair = CreateTestTradingPair();
        var providerIds = new List<int> { 1, 2, 3, 2 }; 

        // Act
        tradingPair.SetExchangeRateProviders(providerIds);

        // Assert
        tradingPair.MarketTradingPairProviders.Should().HaveCount(3);
        tradingPair.MarketTradingPairProviders.Should().OnlyHaveUniqueItems(p => p.ExchangeRateProviderId);
        tradingPair.MarketTradingPairProviders.Select(p => p.ExchangeRateProviderId).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void SetExchangeRateProviders_WithNullList_ShouldThrowArgumentNullException()
    {
        // Arrange
        var tradingPair = CreateTestTradingPair();

        // Act
        Action act = () => tradingPair.SetExchangeRateProviders(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'providerIds')");
    }

    private MarketTradingPair CreateTestTradingPair()
    {
        return new MarketTradingPair(
            marketId: 1,
            currencyId: 1,
            creatorId: 1,
            published: true,
            description: "Test Trading Pair"
        );
    }
}
