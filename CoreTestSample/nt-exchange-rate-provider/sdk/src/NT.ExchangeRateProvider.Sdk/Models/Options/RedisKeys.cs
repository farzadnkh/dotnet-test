namespace NT.SDK.ExchangeRateProvider.Models.Options
{
    public static class RedisKeys
    {
        private static string TokenStoreKeyPrefix = "auth-details";

        public static string GetTokenKey(string cachePrefix)
            => $"{cachePrefix}:{TokenStoreKeyPrefix}";

        public static string GetStoredPriceKey(string cachePrefix, string pair)
    => $"{cachePrefix}:{pair}";
    }
}
