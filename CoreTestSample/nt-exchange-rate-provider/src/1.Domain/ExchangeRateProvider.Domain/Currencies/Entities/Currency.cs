using System.ComponentModel.DataAnnotations.Schema;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.Currencies.Entities;

public class Currency : AuditedEntity<int, User, int>
{
    #region Properties
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Symbol { get; private set; }
    public int? DecimalPrecision { get; private set; }
    public CurrencyType Type { get; private set; }
    public bool Published { get; private set; }

    [NotMapped]
    public string CurrencyName { get; set; }
    public int CreatedById { get; private set; }

    private List<Market> _markets = [];
    public IReadOnlyCollection<Market> Markets => _markets.AsReadOnly();
    
    public ICollection<MarketCurrency> MarketCurrencies { get; set; } = [];
    public ICollection<MarketTradingPair> MarketTradingPairs { get; set; } = [];

    #endregion

    #region Constructors
    private Currency() 
    {
    }

    public Currency(
        string name,
        string code,
        CurrencyType type,
        int createdById,
        int? decimalPrecision = null,
        string symbol = null,
        bool published = false)
    {
        Name = name;
        Code = code;
        Type = type;
        DecimalPrecision = decimalPrecision;
        Published = published;
        CreatedById = createdById;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        Symbol = symbol;
    }
    #endregion

    #region Behaviors
    public static Currency Create(
    string name,
    string code,
    CurrencyType type,
    int createdById,
    int? decimalPrecision = null,
    string symbol = null,
    bool published = false)
    {
        return new Currency(name, code, type, createdById, decimalPrecision, symbol, published);
    }

    public void Update(
        string name,
        int modifierUserId,
        bool published,
        int? decimalPrecision = null,
        string symbol = null,
        List<Market> markets = null)
    {
        Name = name;
        DecimalPrecision = decimalPrecision;
        Symbol = symbol;
        LastModifierUserId = modifierUserId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
        _markets = markets ?? [];
        Published = published;
    }

    public void Deactivate(int modifierUserId)
    {
        Published = false;
        LastModifierUserId = modifierUserId;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void AddMarket(Market market)
    {
        _markets.Add(market);
        MarketCurrencies.Add(new()
        {
            MarketId = market.Id,
            CurrencyId = Id
        });
    }


    #endregion
}

