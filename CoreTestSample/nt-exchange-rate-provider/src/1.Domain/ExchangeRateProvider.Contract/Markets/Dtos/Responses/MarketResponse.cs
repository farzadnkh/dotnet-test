using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Contract.Markets.Dtos.Responses;

public class MarketResponse
{
    public MarketResponse(
        int id,
        int baseCurrencyId,
        MarketCalculationTerm? term,
        bool published,
        bool isDefault,
        RatingMethod ratingMethod,
        string baseCurrencyName = "",
        string baseCurrencyCode = "",
        List<Provider> exchangeProviders = null,
        SpreadOptions spreadOptionRequest = null)
    {
        Id = id;
        Term = term;
        Published = published;
        IsDefault = isDefault;
        BaseCurrencyCode = baseCurrencyCode;
        BaseCurrencyId = baseCurrencyId;
        BaseCurrencyName = baseCurrencyName;
        ExchangeProviders = exchangeProviders;
        SpreadOptions = spreadOptionRequest;
        RatingMethod = ratingMethod;
    }
    public MarketResponse()
    {
    }

    public int Id { get; init; }
    public string Name { get; init; } = $"";
    public string BaseCurrencyCode { get; init; }
    public string BaseCurrencyName { get; init; }
    public int BaseCurrencyId { get; init; }
    public MarketCalculationTerm? Term { get; init; }
    public RatingMethod RatingMethod { get; init; }
    public List<Provider> ExchangeProviders { get; init; }
    public bool Published { get; init; }
    public bool IsDefault { get; init; }

    public SpreadOptions SpreadOptions { get; init; }
}
