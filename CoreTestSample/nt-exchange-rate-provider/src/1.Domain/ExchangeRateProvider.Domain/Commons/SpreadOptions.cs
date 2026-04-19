namespace ExchangeRateProvider.Domain.Commons;

public class SpreadOptions
{
    public bool SpreadEnabled { get; set; }
    public decimal? LowerLimitPercentage { get; set; }
    public decimal? UpperLimitPercentage { get; set; }
}
