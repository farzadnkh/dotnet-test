using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Markets.Entities;

public class MarketTradingPair : FullAuditedEntity<int, User, int>
{
    public int MarketId { get; set; }
    public Market Market { get; set; }

    public int CurrencyId { get; set; }
    public Currency Currency { get; set; }

    public RatingMethod RatingMethod { get; private set; }
    public bool Published { get; private set; }
    public string Description { get; private set; }
    
    public SpreadOptions SpreadOptions { get; private set; } = new();
    public int CreatedById { get; private set; }

    public ICollection<MarketTradingPairProvider> MarketTradingPairProviders { get; set; } = [];

    private MarketTradingPair() { }

    public MarketTradingPair(int marketId, int currencyId, int creatorId, bool published, string description, RatingMethod ratingMethod = RatingMethod.None)
    {
        MarketId = marketId;
        CurrencyId = currencyId;
        CreatedById = creatorId;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        Published = published;
        Description = description;
        RatingMethod = ratingMethod;
    }

    public void EnableSpread(decimal? lower, decimal? upper)
    {
        if (lower < 0 || upper < 0 || lower > upper)
            throw new InvalidOperationException("Invalid spread limits.");

        SpreadOptions = new()
        {
            SpreadEnabled = true,
            LowerLimitPercentage = lower,
            UpperLimitPercentage = upper
        };
    }

    public static MarketTradingPair Create(int marketId, int currencyId, int creatorId, bool published, string description, RatingMethod ratingMethod = RatingMethod.None)
    {
        return new(marketId, currencyId, creatorId, published, description, ratingMethod);
    }

    public void Update(
        int marketId,
        int currencyId,
        SpreadOptions spreadOptions,
        string description,
        bool isPublished,
        RatingMethod ratingMethod = RatingMethod.None)
    {
        MarketId = marketId;
        CurrencyId = currencyId;
        SpreadOptions = spreadOptions;
        Published = isPublished;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
        Description = description;
        RatingMethod = ratingMethod;
    }

    public void SetExchangeRateProviders(List<int> providerIds)
    {
        if (providerIds == null)
            throw new ArgumentNullException(nameof(providerIds));

        MarketTradingPairProviders.Clear();

        foreach (var providerId in providerIds.Distinct())
        {
            MarketTradingPairProviders.Add(new MarketTradingPairProvider
            {
                MarektTradingPairId = Id,
                ExchangeRateProviderId = providerId
            });
        }
    }
}

public static class MarketTradingPairExtensions
{
    public static TradingPairSnapshot ToSnapshot(this MarketTradingPair entity, List<int> providerIds)
    {
        return new TradingPairSnapshot
        {
            Id = entity.Id,
            CurrencyId = entity.CurrencyId,
            CurrencyCode = entity.Currency.Code,
            MarketId = entity.MarketId,
            BaseCurrencyCode = entity.Market.BaseCurrency.Code,
            CalculationTerm = entity.Market.CalculationTerm,
            SpreadOptions = entity.SpreadOptions,
            ProviderIds = providerIds,
            DecimalPrecision = entity.Currency.DecimalPrecision
        };
    }
}

public readonly struct TradingPairSnapshot
{
    public int Id { get; init; }

    public int CurrencyId { get; init; }
    public string CurrencyCode { get; init; }
    public int? DecimalPrecision { get; init; }

    public int MarketId { get; init; }
    public string BaseCurrencyCode { get; init; }
    public MarketCalculationTerm CalculationTerm { get; init; }

    public SpreadOptions SpreadOptions { get; init; }

    public IReadOnlyList<int> ProviderIds  { get; init; }
}

