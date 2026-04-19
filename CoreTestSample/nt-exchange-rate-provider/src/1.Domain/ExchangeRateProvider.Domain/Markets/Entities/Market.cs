using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Markets.Entities;

public class Market : AuditedEntity<int, User, int>
{
    public int BaseCurrencyId { get; private set; }
    public Currency BaseCurrency { get; set; }

    public MarketCalculationTerm CalculationTerm { get; private set; }
    public RatingMethod RatingMethod { get; private set; }
    public bool IsDefault { get; private set; }
    public bool Published { get; private set; }

    public SpreadOptions SpreadOptions { get; private set; } = new();

    public int CreatedById { get; private set; }

    public ICollection<MarketProvider> MarketExchangeRateProviders { get; set; } = [];
    public ICollection<MarketCurrency> MarketCurrencies { get; private set; } = [];
    public ICollection<MarketTradingPair> TradingPairs { get; private set; } = [];

    public Market() { }

    public Market(int baseCurrencyId, MarketCalculationTerm calculationTerm, bool spreadEnabled, int createdById, bool isDefault = false, bool isPublished = false)
    {
        BaseCurrencyId = baseCurrencyId;
        CalculationTerm = calculationTerm;
        SpreadOptions.SpreadEnabled = spreadEnabled;
        IsDefault = isDefault;
        Published = isPublished;
        CreatedById = createdById;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        RatingMethod = RatingMethod.Automatic;
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

    public void AddTradingPair(Currency currency, int creatorId, bool published, string description, RatingMethod ratingMethod = RatingMethod.None)
    {
        var pair = new MarketTradingPair(Id, currency.Id, creatorId, published, description, ratingMethod);
        TradingPairs.Add(pair);
    }

    public void SetPublishMarket(bool isPublished)
    {
        Published = isPublished;
    }

    public void SetDefaultMaket(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public static Market Create(
        int baseCurrencyId,
        MarketCalculationTerm calculationTerm,
        bool spreadEnabled,
        int createdById,
        bool isDefault = false,
        bool isPublished = false)
    {
        return new(baseCurrencyId, calculationTerm, spreadEnabled, createdById, isDefault, isPublished);
    }

    public void Update(
        int baseCurrencyId,
        MarketCalculationTerm calculationTerm,
        SpreadOptions spreadOptions,
        RatingMethod ratingMethod = RatingMethod.Automatic,
        bool isDefault = false,
        bool isPublished = false)
    {
        BaseCurrencyId = baseCurrencyId;
        CalculationTerm = calculationTerm;
        SpreadOptions = spreadOptions ?? new();
        IsDefault = isDefault;
        Published = isPublished;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
        RatingMethod = ratingMethod;
    }

    public void AddExchangeRateProviders(int providerId)
    {
        MarketExchangeRateProviders.Add(new MarketProvider
        {
            MarketId = Id,
            ExchangeRateProviderId = providerId
        });
    }
}

