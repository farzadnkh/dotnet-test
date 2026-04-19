using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Contract.Markets.Dtos.Requests;

public record MarketPaginatedFilterRequest
{
    public int? CurrencyId { get; set; }
    public MarketCalculationTerm? MarketCalculationTerm { get; set; }
    public bool? Published { get; set; }
}
