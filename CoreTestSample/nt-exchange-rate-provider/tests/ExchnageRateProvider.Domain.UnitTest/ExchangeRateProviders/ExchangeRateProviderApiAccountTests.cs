using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using FluentAssertions;

namespace ExchangeRateProvider.Domain.UnitTest.ExchangeRateProviders;

public class ExchangeRateProviderApiAccountTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        const int exchangeRateProviderId = 1;
        const string owner = "John Doe";
        const ProtocolType protocolType = ProtocolType.ApiCall;
        var credentials = new byte[] { 0x01, 0x02, 0x03 };
        const string description = "Test account description.";
        const int createdById = 10;
        const bool published = true;

        // Act
        var apiAccount = new ExchangeRateProviderApiAccount(
            exchangeRateProviderId,
            owner,
            protocolType,
            credentials,
            description,
            createdById,
            published
        );

        // Assert
        apiAccount.ProviderId.Should().Be(exchangeRateProviderId);
        apiAccount.Owner.Should().Be(owner);
        apiAccount.ProtocolType.Should().Be(protocolType);
        apiAccount.Credentials.Should().BeEquivalentTo(credentials);
        apiAccount.Description.Should().Be(description);
        apiAccount.CreatedById.Should().Be(createdById);
        apiAccount.Published.Should().Be(published);
    }

    [Fact]
    public void Deactivate_ShouldSetPublishedToFalse()
    {
        // Arrange
        var apiAccount = new ExchangeRateProviderApiAccount(
            exchangeRateProviderId: 1,
            owner: "Test Owner",
            protocolType: ProtocolType.WebSocket,
            credentials: [],
            description: "Test",
            createdById: 1,
            published: true
        );
        apiAccount.Published.Should().BeTrue();
        const int modifierUserId = 2;

        // Act
        apiAccount.Deactivate(modifierUserId);

        // Assert
        apiAccount.Published.Should().BeFalse();
        apiAccount.LastModifierUserId.Should().Be(modifierUserId);
        apiAccount.UpdatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithNewValues_ShouldChangeAllProperties()
    {
        // Arrange
        var apiAccount = new ExchangeRateProviderApiAccount(
            exchangeRateProviderId: 1,
            owner: "Old Owner",
            protocolType: ProtocolType.WebSocket,
            credentials: [0x00],
            description: "Old",
            createdById: 1,
            published: false
        );

        const int newExchangeRateProviderId = 2;
        const string newOwner = "New Owner";
        const ProtocolType newProtocolType = ProtocolType.ApiCall;
        var newCredentials = new byte[] { 0x01, 0x02 };
        const string newDescription = "New Description";
        const int newCreatedById = 2;
        const bool newPublished = true;

        // Act
        apiAccount.Update(
            newExchangeRateProviderId,
            newOwner,
            newProtocolType,
            newCredentials,
            newDescription,
            newCreatedById,
            newPublished
        );

        // Assert
        apiAccount.ProviderId.Should().Be(newExchangeRateProviderId);
        apiAccount.Owner.Should().Be(newOwner);
        apiAccount.ProtocolType.Should().Be(newProtocolType);
        apiAccount.Credentials.Should().BeEquivalentTo(newCredentials);
        apiAccount.Description.Should().Be(newDescription);
        apiAccount.CreatedById.Should().Be(newCreatedById);
        apiAccount.Published.Should().Be(newPublished);
    }
}
