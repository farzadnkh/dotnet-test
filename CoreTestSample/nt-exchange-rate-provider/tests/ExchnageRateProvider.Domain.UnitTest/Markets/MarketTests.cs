using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.Markets;

public class MarketTests
{
    private Market CreateTestMarket()
    {
        return new Market(
            baseCurrencyId: 1,
            calculationTerm: MarketCalculationTerm.Average,
            spreadEnabled: false,
            createdById: 1,
            isDefault: false,
            isPublished: true
        );
    }

    [Fact]
    public void Create_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        const int baseCurrencyId = 10;
        const MarketCalculationTerm calculationTerm = MarketCalculationTerm.Average;
        const bool spreadEnabled = true;
        const int createdById = 1;
        const bool isDefault = true;
        const bool isPublished = false;

        // Act
        var market = Market.Create(baseCurrencyId, calculationTerm, spreadEnabled, createdById, isDefault, isPublished);

        // Assert
        market.BaseCurrencyId.Should().Be(baseCurrencyId);
        market.CalculationTerm.Should().Be(calculationTerm);
        market.SpreadOptions.SpreadEnabled.Should().Be(spreadEnabled);
        market.CreatedById.Should().Be(createdById);
        market.IsDefault.Should().Be(isDefault);
        market.Published.Should().Be(isPublished);
        market.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // Behavior Tests
    [Fact]
    public void EnableSpread_WithValidLimits_ShouldSetSpreadOptions()
    {
        // Arrange
        var market = CreateTestMarket();
        const decimal lower = 5.0m;
        const decimal upper = 10.0m;

        // Act
        market.EnableSpread(lower, upper);

        // Assert
        market.SpreadOptions.SpreadEnabled.Should().BeTrue();
        market.SpreadOptions.LowerLimitPercentage.Should().Be(lower);
        market.SpreadOptions.UpperLimitPercentage.Should().Be(upper);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    [InlineData(10, 5)]
    public void EnableSpread_WithInvalidLimits_ShouldThrowInvalidOperationException(decimal lower, decimal upper)
    {
        // Arrange
        var market = CreateTestMarket();

        // Act
        Action act = () => market.EnableSpread(lower, upper);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid spread limits.");
    }

    [Fact]
    public void AddTradingPair_ShouldAddPairToCollection()
    {
        // Arrange
        var market = CreateTestMarket();
        var currency = new Currency("Test", "TST", CurrencyType.Crypto, 1);
        const int creatorId = 1;
        const bool published = true;
        const string description = "Test Pair";

        // Act
        market.AddTradingPair(currency, creatorId, published, description);

        // Assert
        market.TradingPairs.Should().ContainSingle();
        var addedPair = market.TradingPairs.First();
        addedPair.CurrencyId.Should().Be(currency.Id);
        addedPair.Published.Should().Be(published);
        addedPair.Description.Should().Be(description);
    }

    [Fact]
    public void SetPublishMarket_ShouldUpdatePublishedState()
    {
        // Arrange
        var market = CreateTestMarket();
        market.Published.Should().BeTrue();

        // Act
        market.SetPublishMarket(false);

        // Assert
        market.Published.Should().BeFalse();
    }

    [Fact]
    public void SetDefaultMaket_ShouldUpdateIsDefaultState()
    {
        // Arrange
        var market = CreateTestMarket();
        market.IsDefault.Should().BeFalse();

        // Act
        market.SetDefaultMaket(true);

        // Assert
        market.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Update_WithNewValues_ShouldChangeProperties()
    {
        // Arrange
        var market = CreateTestMarket();
        const int newBaseCurrencyId = 2;
        const MarketCalculationTerm newTerm = MarketCalculationTerm.Average;
        var newSpreadOptions = new SpreadOptions { SpreadEnabled = true, LowerLimitPercentage = 1, UpperLimitPercentage = 2 };
        const bool newIsDefault = true;
        const bool newIsPublished = false;
        var ratingMethod = RatingMethod.Automatic;

        // Act
        market.Update(newBaseCurrencyId, newTerm, newSpreadOptions, ratingMethod, newIsDefault, newIsPublished);

        // Assert
        market.BaseCurrencyId.Should().Be(newBaseCurrencyId);
        market.CalculationTerm.Should().Be(newTerm);
        market.SpreadOptions.Should().BeEquivalentTo(newSpreadOptions);
        market.IsDefault.Should().Be(newIsDefault);
        market.RatingMethod.Should().Be(ratingMethod);
        market.Published.Should().Be(newIsPublished);
        market.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddExchangeRateProviders_ShouldAddProviderToCollection()
    {
        // Arrange
        var market = CreateTestMarket();
        const int providerId = 5;

        // Act
        market.AddExchangeRateProviders(providerId);

        // Assert
        market.MarketExchangeRateProviders.Should().ContainSingle();
        var addedProvider = market.MarketExchangeRateProviders.First();
        addedProvider.ExchangeRateProviderId.Should().Be(providerId);
    }
}
