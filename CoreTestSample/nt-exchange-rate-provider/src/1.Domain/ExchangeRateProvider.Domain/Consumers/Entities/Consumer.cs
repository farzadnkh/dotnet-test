using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Consumers.Entities;

public class Consumer : AuditedEntity<int, User, int>
{
    #region Properties
    public int UserId { get; private set; }
    public User User { get; set; }
    public string ProjectName { get; private set; }
    public bool IsActive { get; private set; }
    public string Apikey { get; private set; }
    public int CreatedById { get; private set; }
    public string WhiteListIps { get; private set; }

    public ICollection<ConsumerMarket> ConsumerMarkets { get; set; } = [];
    public ICollection<ConsumerProvider> ConsumerProviders { get; set; } = [];
    public ICollection<ConsumerPair> ConsumerPairs { get; set; } = [];
    #endregion

    #region Constructors

    private Consumer() { }

    public Consumer(int userId, string projectName, int createdById)
    {
        UserId = userId;
        ProjectName = projectName;
        IsActive = true;
        CreatedById = createdById;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        CreatorUserId = createdById;
        LastModifierUserId = createdById;
    }

    #endregion

    #region Behaviors

    public void Update(string projectName, bool isActive)
    {
        ProjectName = projectName;
        IsActive = isActive;

        LastModifierUserId = 1;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public User UpdateUserDetails(string userName, string email, string firstName, string lastName)
    {
        User.FirstName = firstName;
        User.UserName = userName;
        User.Email = email;
        User.LastName = lastName;

        return User;
    }

    public void Deactivate(int modifierUserId)
    {
        IsActive = false;
        LastModifierUserId = modifierUserId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void Activate(int modifierUserId)
    {
        IsActive = true;
        LastModifierUserId = modifierUserId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void AddProvider(int providerId, bool isActive, int createdById)
    {
        ConsumerProviders.Add(new(
            Id,
            providerId,
            isActive,
            createdById,
            DateTime.UtcNow));
    }

    public void AddMarket(int marketId, int createdById, bool isActive, SpreadOptions spreadOptions)
    {
        var market = new ConsumerMarket(
            Id,
            marketId,
            isActive,
            createdById,
            DateTime.UtcNow);

        market.SetSpreadLimits(spreadOptions);
        ConsumerMarkets.Add(market);
    }

    public void AddTradingPairs(int tradingPairId, int marketId, int createdById, bool isActive, SpreadOptions spreadOptions)
    {
        ConsumerPair consumerPair = new(
            Id,
            marketId,
            tradingPairId,
            isActive,
            createdById);

        consumerPair.SetSpreadLimits(spreadOptions);
        ConsumerPairs.Add(consumerPair);
    }

    public static Consumer Create(int userId, string projectName, int createdById)
        => new(userId, projectName, createdById);

    public void ClearAssociations()
    {
        ConsumerMarkets = [];
        ConsumerPairs = [];
        ConsumerProviders = [];
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetWhiteListIps(string ipAddresses)
    {
        if (!string.IsNullOrEmpty(ipAddresses))
            WhiteListIps = ipAddresses;
    }

    public string GetWhiteListIps()
    {
        if(!string.IsNullOrEmpty(WhiteListIps))
            return WhiteListIps;

        return string.Empty;
    }
    #endregion
}

public static class ConsumerExtensions
{
    public static ConsumerSnapshot ToSnapshot(this Consumer consumer)
    {
        return new ConsumerSnapshot
        {
            Id = consumer.Id,
            ProjectName = consumer.ProjectName,
            IsActive = consumer.IsActive,
            Apikey = consumer.Apikey,
            UserId = consumer.User.Id,
            UserName = consumer.User.UserName,
            UserEmail = consumer.User.Email,
            UserFirstName = consumer.User.FirstName,
            UserLastName = consumer.User.LastName,
            MarketIds = consumer.ConsumerMarkets.Select(m => m.MarketId).ToList(),
            ProviderIds = consumer.ConsumerProviders.Select(p => p.ProviderId).ToList(),
            PairIds = consumer.ConsumerPairs
                .Select(p => (p.MarketId, p.MarketTradingPair.Id)).ToList()
        };
    }
}

public readonly struct ConsumerSnapshot
{
    public int Id { get; init; }
    public string ProjectName { get; init; }
    public bool IsActive { get; init; }
    public string Apikey { get; init; }

    public int UserId { get; init; }
    public string UserName { get; init; }
    public string UserEmail { get; init; }
    public string UserFirstName { get; init; }
    public string UserLastName { get; init; }

    public List<int> MarketIds { get; init; }
    public List<int> ProviderIds { get; init; }
    public List<(int MarketId, int PairId)> PairIds { get; init; }
}
