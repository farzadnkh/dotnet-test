using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

public class Provider : AuditedEntity<int, User, int>
{
    #region Properties
    public string Name { get; private set; }
    public ProviderType Type { get; private set; }
    public bool Published { get; private set; }
    public int CreatedById { get; private set; }

    public ICollection<ExchangeRateProviderApiAccount> ApiAccounts { get; set; } = [];
    public ICollection<MarketProvider> MarketExchangeRateProviders { get; set; } = [];
    public ICollection<MarketTradingPairProvider> MarketTradingPairProviders { get; set; } = [];
    public ICollection<ProviderBusinessLogic> ProviderBusinessLogics { get; set; } = [];
    #endregion

    #region Constructors
    private Provider() { }

    public Provider(string name, ProviderType type, int createdById)
    {
        Name = name;
        Type = type;
        Published = true;
        CreatedById = createdById;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        CreatorUserId = createdById;
    }
    #endregion

    #region Behaviors

    public static Provider Create(string name, ProviderType type, int createdById, bool published = true)
        => new(name, type, createdById);

    public void Update(string name, ProviderType type, bool published, int modifierId)
    {
        Name = name;
        Type = type;
        Published = published;
        LastModifierUserId = modifierId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void SetPublishProvider(bool isPublished)
    {
        Published = isPublished;
    }
    
    public void AddProviderBusinessLogics(string name, int creatorUserId)
    {
        ProviderBusinessLogics.Add(new ProviderBusinessLogic()
        {
            ProviderId = Id,
            CreatorUserId = creatorUserId,
            Name = name
        });
    }

    public void RemoveAllLogics()
    {
        ProviderBusinessLogics = [];
    }
    
    #endregion
}
