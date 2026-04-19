namespace ExchangeRateProvider.Domain.Commons.Dtos;

public class PairExchangeRateDto(string currencyPair, decimal price, decimal? upperLimit, decimal? lowerLimit, decimal? upperLimitPercentage, decimal? lowerLimitPercentage)
{
    public string CurrencyPair { get; set; } = currencyPair;
    public decimal Price { get; set; } = price;
    public decimal? UpperLimit { get; set; } = upperLimit;
    public decimal? LowerLimit { get; set; } = lowerLimit;
    public decimal? UpperLimitPercentage { get; set; } = upperLimitPercentage ?? 0;
    public decimal? LowerLimitPercentage { get; set; } = lowerLimitPercentage ?? 0;
}