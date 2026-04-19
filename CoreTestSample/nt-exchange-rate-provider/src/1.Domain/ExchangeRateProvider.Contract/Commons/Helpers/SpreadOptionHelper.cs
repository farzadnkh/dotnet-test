using ExchangeRateProvider.Domain.Commons;

namespace ExchangeRateProvider.Contract.Commons.Helpers
{
    public static class SpreadOptionHelper
    {
        public static (decimal UpperLimit, decimal LowerLimit) GetSpreadPrice(this SpreadOptions spreadOptions, decimal price)
        {
            var upperLimitPercentage = price + MathHelper.Ratio(price * spreadOptions?.UpperLimitPercentage ?? 0, 100);
            var lowerLimitPercentage = price - MathHelper.Ratio(price * spreadOptions?.LowerLimitPercentage ?? 0, 100);

            return (upperLimitPercentage, lowerLimitPercentage);
        }
    }
}
