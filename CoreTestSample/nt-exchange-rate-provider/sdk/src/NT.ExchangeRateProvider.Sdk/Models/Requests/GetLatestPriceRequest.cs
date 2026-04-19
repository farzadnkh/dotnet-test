using NT.SDK.ExchangeRateProvider.Models.Enums;
using NT.SDK.RestClient.Models;

namespace NT.SDK.ExchangeRateProvider.Models.Requests
{
    public class GetLatestPriceRequest : IRequestBody
    {
        /// <summary>
        /// Market is basically the Quote Currency, This Filed is not required.
        /// if you want to send A list of the Markets it should use comma separator
        /// </summary>
        /// <example>USDT</example>
        /// <example>USDT,BTC</example> 
        public string Market { get; set; } = null;

        /// <summary>
        /// Pairs is basically the BaseCurrency, This Filed is not required.
        /// if you want to send A list of the Pairs it should use comma separator
        /// </summary>
        /// <example>LTC</example>
        /// <example>LTC,BTC</example> 
        public string Pairs { get; set; } = null;

        /// <summary>
        /// ProviderTypes, This Filed is not required.
        /// </summary>
        /// <example>3</example>
        public ProviderType? ProviderTypes { get; set; } = ProviderType.None;

        /// <summary>
        /// Use for Caching The returned Price, This Filed is not required.
        /// </summary>
        public bool? EnableCache { get; set; } = false;

        /// <summary>
        /// Sets Time To Live For Cache, This Filed is not required.
        /// </summary>
        public int? CacheTtlInSec { get; set; } = 10;
    }
}
