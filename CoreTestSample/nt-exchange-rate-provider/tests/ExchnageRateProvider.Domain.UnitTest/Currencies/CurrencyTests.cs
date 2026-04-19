using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.Currencies
{
    public class CurrencyTests
    {
        [Fact]
        public void Create_WithValidParameters_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            const string name = "US Dollar";
            const string code = "USD";
            const CurrencyType type = CurrencyType.Fiat;
            const int createdById = 1;
            const int decimalPrecision = 2;
            const string symbol = "$";
            const bool published = true;

            // Act
            var currency = Currency.Create(name, code, type, createdById, decimalPrecision, symbol, published);

            // Assert
            currency.Name.Should().Be(name);
            currency.Code.Should().Be(code);
            currency.Type.Should().Be(type);
            currency.CreatedById.Should().Be(createdById);
            currency.DecimalPrecision.Should().Be(decimalPrecision);
            currency.Symbol.Should().Be(symbol);
            currency.Published.Should().Be(published);
            currency.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Update_WithNewValues_ShouldChangeProperties()
        {
            // Arrange
            var currency = CreateTestCurrency();
            const string newName = "New Bitcoin Name";
            const int modifierUserId = 2;
            const bool published = true;
            const int newDecimalPrecision = 8;
            const string newSymbol = "฿";
            var newMarkets = new List<Market> { new() };

            // Act
            currency.Update(newName, modifierUserId, published, newDecimalPrecision, newSymbol, newMarkets);

            // Assert
            currency.Name.Should().Be(newName);
            currency.LastModifierUserId.Should().Be(modifierUserId);
            currency.Published.Should().Be(published);
            currency.DecimalPrecision.Should().Be(newDecimalPrecision);
            currency.Symbol.Should().Be(newSymbol);
            currency.Markets.Should().BeEquivalentTo(newMarkets);
            currency.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Deactivate_ShouldSetPublishedToFalse()
        {
            // Arrange
            var currency = CreateTestCurrency();
            currency.Update("Bitcoin", 2, true);
            currency.Published.Should().BeTrue();
            const int modifierUserId = 3;

            // Act
            currency.Deactivate(modifierUserId);

            // Assert
            currency.Published.Should().BeFalse();
            currency.LastModifierUserId.Should().Be(modifierUserId);
            currency.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        }

        private Currency CreateTestCurrency() =>
            new(
                name: "Bitcoin",
                code: "BTC",
                type: CurrencyType.Crypto,
                createdById: 1,
                symbol: "₿"
            );
    }
}
