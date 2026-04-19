namespace ExchangeRateProvider.Contract.Markets.Dtos.Responses;

public class ManualRatingResponse
{
    public ManualRatingResponse(
        int id,
        string tradingPair,
        decimal? price,
        decimal? upperLimit,
        decimal? lowerLimit,
        decimal? lowerLimitPercentage,
        decimal? upperLimitPercentage,
        decimal oldPrice = 0)
    {
        TradingPairId = id;
        TradingPair = tradingPair;
        Price = price;
        UpperLimit = upperLimit;
        LowerLimit = lowerLimit;
        LowerLimitPercentage = lowerLimitPercentage;
        UpperLimitPercentage = upperLimitPercentage;
        OldPrice = oldPrice;
    }

    public ManualRatingResponse()
    {
    }

    public int TradingPairId { get; init; }
    public string TradingPair { get; init; }
    public decimal? Price { get; init; }
    public decimal? UpperLimit { get; init; }
    public decimal? LowerLimit { get; init; }
    public decimal? LowerLimitPercentage { get; init; }
    public decimal? UpperLimitPercentage { get; init; }
    public decimal OldPrice { get; init; }
}
