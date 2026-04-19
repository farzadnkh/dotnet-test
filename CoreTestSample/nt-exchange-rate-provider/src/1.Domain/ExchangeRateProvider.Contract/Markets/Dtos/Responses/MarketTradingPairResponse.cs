using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Contract.Markets.Dtos.Responses;

public class MarketTradingPairResponse
{
    public MarketTradingPairResponse(
        int id, 
        string pair, 
        string currency, 
        bool published, 
        SpreadOptions spreadOptions,
        string description,
        Market market = null,
        Currency currencyEntity = null,
        List<Provider> exchangeProviders = null,
        RatingMethod ratingMethod = RatingMethod.None)
    {
        Id = id;
        Published = published;
        Currency = currency;
        Pair = pair;
        SpreadOptions = spreadOptions;
        ExchangeProviders = exchangeProviders;
        Market = market;
        CurrencyEntity = currencyEntity;
        Description = description;
        RatingMethod = ratingMethod;
    }
    public MarketTradingPairResponse()
    {
    }

    public int Id { get; init; }
    public string Pair { get; init; }
    public string Currency { get; init; }
    public bool Published { get; init; }  
    public SpreadOptions SpreadOptions { get; init; }
    public List<Provider> ExchangeProviders { get; init; }
    public Market Market { get; init; }
    public Currency CurrencyEntity { get; init; }
    public RatingMethod RatingMethod { get; init; }
    public string Description { get; init; }
}
