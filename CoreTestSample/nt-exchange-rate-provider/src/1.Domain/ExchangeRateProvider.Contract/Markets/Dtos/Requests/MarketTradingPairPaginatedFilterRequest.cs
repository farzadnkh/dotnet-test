namespace ExchangeRateProvider.Contract.Markets.Dtos.Requests;

public record MarketTradingPairPaginatedFilterRequest
{
    public int? MarketId { get; set; }
    public int? CurrencyId { get; set; }
    public bool? Published { get; set; }
}
