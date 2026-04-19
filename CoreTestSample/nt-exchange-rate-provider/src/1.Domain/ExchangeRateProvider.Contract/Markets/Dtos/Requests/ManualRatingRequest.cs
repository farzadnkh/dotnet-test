namespace ExchangeRateProvider.Contract.Markets.Dtos.Requests;

public record ManualRatingRequest(int PairId, string TradingPair, decimal? NewPrice)
{
    public decimal OldPrice {  get; set; }
}
