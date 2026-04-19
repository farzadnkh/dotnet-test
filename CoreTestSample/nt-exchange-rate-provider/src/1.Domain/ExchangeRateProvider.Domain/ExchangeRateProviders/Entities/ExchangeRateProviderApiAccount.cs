using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

public class ExchangeRateProviderApiAccount : AuditedEntity<int, User, int>
{
    #region Properties
    public int ProviderId { get; set; }
    public Provider ExchangeRateProvider { get; set; }

    public ProtocolType ProtocolType { get; private set; }
    public string Owner { get; private set; }
    public byte[] Credentials { get; private set; }
    public bool Published { get; private set; }
    public string Description { get; private set; }
    public int CreatedById { get; private set; }

    #endregion

    #region Constructors
    public ExchangeRateProviderApiAccount()
    {
    }

    public ExchangeRateProviderApiAccount(int exchangeRateProviderId, string owner, ProtocolType protocolType, byte[] credentials, string description, int createdById, bool published)
    {
        Owner = owner;
        ProtocolType = protocolType;
        Credentials = credentials;
        Description = description;
        CreatedById = createdById;
        Published = published;
        ProviderId = exchangeRateProviderId;
    }
    #endregion

    #region Behaviors
    public void Deactivate(int modifierUserId)
    {
        Published = false;
        LastModifierUserId = modifierUserId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void Update(int exchangeRateProviderId, string owner, ProtocolType protocolType, byte[] credentials, string description, int createdById, bool published)
    {
        Owner = owner;
        ProtocolType = protocolType;
        Credentials = credentials;
        Description = description;
        CreatedById = createdById;
        Published = published;
        ProviderId = exchangeRateProviderId;
    }
    #endregion
}
