using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.ExchangeRateProviders;

public class ProviderTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        const string name = "XE";
        const ProviderType type = ProviderType.XE;
        const int createdById = 10;

        // Act
        var provider = Provider.Create(name, type, createdById);

        // Assert
        provider.Name.Should().Be(name);
        provider.Type.Should().Be(type);
        provider.Published.Should().BeTrue();
        provider.CreatedById.Should().Be(createdById);
        provider.CreatorUserId.Should().Be(createdById);
        provider.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithNewValues_ShouldChangeProperties()
    {
        // Arrange
        var provider = CreateTestProvider();
        const string newName = "Updated Provider";
        const ProviderType newType = ProviderType.XE;
        const bool isPublished = false;
        const int modifierId = 2;

        // Act
        provider.Update(newName, newType, isPublished, modifierId);

        // Assert
        provider.Name.Should().Be(newName);
        provider.Type.Should().Be(newType);
        provider.Published.Should().Be(isPublished);
        provider.LastModifierUserId.Should().Be(modifierId);
        provider.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetPublishProvider_ShouldChangePublishedState(bool isPublished)
    {
        // Arrange
        var provider = CreateTestProvider();

        // Act
        provider.SetPublishProvider(isPublished);

        // Assert
        provider.Published.Should().Be(isPublished);
    }

    private Provider CreateTestProvider()
    {
        return new Provider(
            name: "CryptoCompare",
            type: ProviderType.CryptoCompare,
            createdById: 1
        );
    }
}
