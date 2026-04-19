using ExchangeRateProvider.Contract.Commons.Dtos;
using ExchangeRateProvider.Contract.Commons.Helpers;

namespace ExchangeRateProvider.Application.Consumers.Extensions;

public static class ExchangeRateFilterHelper
{
    private const long DefaultMaxAgeInTicks = 10_000;
    private const decimal DefaultMaxAllowedRatioPercent = 0.01m;

    public static List<ExchangeRateValue> FilterOutliers(
        List<ExchangeRateValue> rates,
        long maxAgeInTicks,
        decimal maxAllowedRatioDiff
    )
    {
        if (rates == null || rates.Count == 0)
            return [];

        if (maxAgeInTicks <= 0)
            maxAgeInTicks = DefaultMaxAgeInTicks;

        if (maxAllowedRatioDiff <= 0)
            maxAllowedRatioDiff = DefaultMaxAllowedRatioPercent;
        else
            maxAllowedRatioDiff = MathHelper.Ratio(maxAllowedRatioDiff, 100);

        long currentUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var recentRates = rates
            .Where(rate => currentUnixSeconds - rate.Ticks <= maxAgeInTicks)
            .ToList();

        return recentRates.Count switch
        {
            < 2 => recentRates,
            2 => FilterOutliersFromTwoRates(recentRates, maxAllowedRatioDiff),
            3 => FilterOutliersFromThreeRates(recentRates, maxAllowedRatioDiff),
            _ => FilterOutliersUsingIqrMethod(recentRates, currentUnixSeconds)
        };
    }

    private static List<ExchangeRateValue> FilterOutliersFromTwoRates(List<ExchangeRateValue> rates, decimal maxRatioDiff)
    {
        var firstPrice = rates[0].Price;
        var secondPrice = rates[1].Price;
        var ratio = firstPrice > secondPrice ? firstPrice / secondPrice : secondPrice / firstPrice;

        return ratio > 1 + maxRatioDiff
            ? [rates.OrderByDescending(r => r.Ticks).First()]
            : rates;
    }

    private static List<ExchangeRateValue> FilterOutliersFromThreeRates(List<ExchangeRateValue> rates, decimal maxRatioDiff)
    {
        var sortedRates = rates.OrderBy(r => r.Price).ToList();
        var median = sortedRates[1].Price;

        var lowerDiff = Math.Abs(sortedRates[0].Price - median) / median;
        var upperDiff = Math.Abs(sortedRates[2].Price - median) / median;

        if (lowerDiff > maxRatioDiff && upperDiff <= maxRatioDiff)
            return [sortedRates[1], sortedRates[2]];

        if (upperDiff > maxRatioDiff && lowerDiff <= maxRatioDiff)
            return [sortedRates[0], sortedRates[1]];

        return sortedRates;
    }

    private static List<ExchangeRateValue> FilterOutliersUsingIqrMethod(List<ExchangeRateValue> rates, long nowUnixSeconds)
    {
        var prices = rates.Select(r => r.Price).OrderBy(p => p).ToList();
        var ages = rates.Select(r => nowUnixSeconds - r.Ticks).OrderBy(a => a).ToList();

        var (minPrice, maxPrice) = CalculateIqrRange(prices);
        var (minAge, maxAge) = CalculateIqrRange(ages);

        return [.. rates
            .Where(r =>
            {
                var age = nowUnixSeconds - r.Ticks;
                return r.Price >= minPrice && r.Price <= maxPrice &&
                       age >= minAge && age <= maxAge;
            })];
    }

    private static (decimal min, decimal max) CalculateIqrRange(List<decimal> sorted)
    {
        var firstQuartile = CalculatePercentile(sorted, 0.25);
        var thirdQuartile = CalculatePercentile(sorted, 0.75);
        var iqr = thirdQuartile - firstQuartile;
        return (firstQuartile - 1.5m * iqr, thirdQuartile + 1.5m * iqr);
    }

    private static (long min, long max) CalculateIqrRange(List<long> sorted)
    {
        var firstQuartile = CalculatePercentile(sorted, 0.25);
        var thirdQuartile = CalculatePercentile(sorted, 0.75);
        var iqr = thirdQuartile - firstQuartile;
        return (firstQuartile - (long)(1.5 * iqr), thirdQuartile + (long)(1.5 * iqr));
    }

    private static decimal CalculatePercentile(List<decimal> sorted, double percentile)
    {
        var sortedCount = sorted.Count;
        var n = (sortedCount - 1) * percentile;
        var low = (int)Math.Floor(n);
        var high = (int)Math.Ceiling(n);
        if (low == high) return sorted[low];
        return sorted[low] + (decimal)(n - low) * (sorted[high] - sorted[low]);
    }

    private static long CalculatePercentile(List<long> sorted, double percentile)
    {
        var sortedCount = sorted.Count;
        var n = (sortedCount - 1) * percentile;
        var low = (int)Math.Floor(n);
        var high = (int)Math.Ceiling(n);
        if (low == high) return sorted[low];
        return sorted[low] + (long)((n - low) * (sorted[high] - sorted[low]));
    }
}