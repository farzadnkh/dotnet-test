using ExchangeRateProvider.Domain.Currencies.Enums;

namespace ExchangeRateProvider.Contract.Currencies.Dtos.Responses;

public class CurrencyResponse
{
    public CurrencyResponse(
        int id,
        string name,
        string code,
        CurrencyType type,
        int createdById,
        bool published,
        List<int> marketIds,
        int? lastModifierUserId,
        DateTimeOffset createdOnUtc,
        DateTimeOffset? updatedOnUtc,
        int? decimalPrecision = null,
        string symbol = null)
    {
        Id = id;
        Name = name;
        Code = code;
        Type = type;
        CreatedById = createdById;
        Published = published;
        LastModifierUserId = lastModifierUserId;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
        DecimalPrecision = decimalPrecision;
        Symbol = symbol;
        MarketIds = marketIds;
    }
    public CurrencyResponse()
    {
    }

    public int Id { get; init; }
    public string Name { get; init; }
    public string Code { get; init; }
    public CurrencyType Type { get; init; }
    public int CreatedById { get; init; }
    public bool Published { get; init; }
    public int? LastModifierUserId { get; init; }
    public DateTimeOffset CreatedOnUtc { get; init; }
    public DateTimeOffset? UpdatedOnUtc { get; init; }
    public int? DecimalPrecision { get; init; }
    public string Symbol { get; init; }
    public List<int> MarketIds { get; init; } = [];
}
