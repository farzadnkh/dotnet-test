using ExchangeRateProvider.Domain.Currencies.Enums;

namespace ExchangeRateProvider.Contract.Currencies.Dtos.Requests;

public record CreateCurrencyRequest(
    string Name,
    string Code,
    CurrencyType Type,
    int CreatedById,
    int? DecimalPrecision = null,
    bool Published = false,
    string Symbol = null,
    List<int> MarketIds = null);
