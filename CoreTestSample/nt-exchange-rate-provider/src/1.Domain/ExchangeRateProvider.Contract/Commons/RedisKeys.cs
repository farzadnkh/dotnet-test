using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.Commons
{
    public static class RedisKeys
    {
        #region Methods

        public static string GeneratePairHashKey(string pair) => $"{BaseKey}:{pair}";
        public static string GeneratePairKeys(ProviderType providerType, string market) 
        {
            if(!string.IsNullOrEmpty(market))
               return $"{providerType}:{market}";

            return $"{providerType}";
        } 
        public static string GenerateRedisKeyForConsumerMarketPairs(int consumerId) => $"{BaseKey}:{consumerId}:MarketPairs";

        public static string GenerateTradingPairKey(string marketCurrencyName, string pairCurrencyName) =>
            $"{BaseKey}:{pairCurrencyName}{marketCurrencyName}";

        public static string GenerateTradingPairApiCacheKey(int consumerId) => $"{BaseKey}:PairResult:{consumerId}";

        public static string DeActivatedConsumers() => $"{BaseKey}:DeActivatedConsumers";
        #endregion


        #region Properties
        private static string BaseKey => "ExchangeRate";
        #endregion
    }
}