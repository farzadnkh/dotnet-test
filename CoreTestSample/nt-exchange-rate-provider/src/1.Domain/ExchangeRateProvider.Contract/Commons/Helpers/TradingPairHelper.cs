using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Contract.Commons.Helpers;

public static class TradingPairHelper
{
    public static string GetPairFormated(this MarketTradingPair pair)
    {
        if (pair is null)
            return "";

        return $"{pair.Currency.Code}/{pair.Market.BaseCurrency.Code}";
    }
}
