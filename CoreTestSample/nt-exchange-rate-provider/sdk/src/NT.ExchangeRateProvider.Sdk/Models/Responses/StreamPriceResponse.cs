using NT.SDK.ExchangeRateProvider.Models.Enums;

namespace NT.SDK.ExchangeRateProvider.Models.Responses
{
    public class StreamPriceResponse
    {
        public SocketMessageType Type { get; set; }
        public string Pair { get; set; }
        public decimal Price { get; set; }

        /// <summary>
        /// Ask = upper price
        /// </summary>
        public decimal? Ask { get; set; }
        public decimal? AskSpreadPercentage { get; set; }

        /// <summary>
        /// Bid = lower price
        /// </summary>
        public decimal? Bid { get; set; }
        public decimal? BidSpreadPercentage { get; set; }
        public string Message { get; set; }
    }
}
