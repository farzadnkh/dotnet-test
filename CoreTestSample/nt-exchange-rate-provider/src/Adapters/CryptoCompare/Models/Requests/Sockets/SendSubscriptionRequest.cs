using Newtonsoft.Json;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Models.Requests.Sockets
{
    public class SendSubscriptionRequest
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// When requesting trades you can filter by specific groups of interest. If left empty it will get all data that your account is allowed to access.
        /// </summary>
        [JsonProperty("groups")]
        public List<string> Groups { get; set; } = [];

        /// <summary>
        /// An array of mapped and/or unmapped instruments to retrieve for a specific market
        /// (you can use either the instrument XXBTZUSD or mapped instrument (base - quote) BTC-USD on kraken as an example). We return the mapped version of the values by default.
        /// </summary>
        [JsonProperty("instruments")]
        public List<string> Instruments { get; set; } = [];

        /// <summary>
        /// The exchange to obtain data from
        /// </summary>
        [JsonProperty("market")]
        public string Market { get; set; }

        /// <summary>
        /// Determines if provided instrument values are converted according 
        /// to internal mappings. When true, values are translated (e.g., coinbase 'USDT-USDC' becomes 'USDC-USDT' and we invert the values); when false, original values are used.
        /// </summary>
        [JsonProperty("apply_mapping")]
        public bool ApplyMapping { get; set; }

        /// <summary>
        /// Tick messages are sent at most once per second. Custom tick intervals can be specified when this feature is enabled on private enterprise streamers.
        /// </summary>
        [JsonProperty("tick_interval")]
        public int TickInterval { get; set; }
    }
}
