using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Models.Responses.Sockets
{
    public class AggregateTickMessageResponse : InboundMessageBase
    {
        [JsonProperty("MARKET")]
        public string Market { get; set; }

        [JsonProperty("INSTRUMENT")]
        public string Instrument { get; set; }

        [JsonProperty("MAPPED_INSTRUMENT")]
        public string MappedInstrument { get; set; }

        [JsonProperty("CCSEQ")]
        public long Ccseq { get; set; }

        [JsonProperty("CURRENT_HOUR_VOLUME")]
        public decimal CurrentHourVolume { get; set; }

        [JsonProperty("CURRENT_HOUR_VOLUME_BUY")]
        public decimal CurrentHourVolumeBuy { get; set; }

        [JsonProperty("CURRENT_HOUR_VOLUME_SELL")]
        public decimal CurrentHourVolumeSell { get; set; }

        [JsonProperty("CURRENT_HOUR_VOLUME_UNKNOWN")]
        public decimal CurrentHourVolumeUnknown { get; set; }

        [JsonProperty("CURRENT_HOUR_QUOTE_VOLUME")]
        public decimal CurrentHourQuoteVolume { get; set; }

        [JsonProperty("CURRENT_HOUR_QUOTE_VOLUME_BUY")]
        public decimal CurrentHourQuoteVolumeBuy { get; set; }

        [JsonProperty("CURRENT_HOUR_QUOTE_VOLUME_SELL")]
        public decimal CurrentHourQuoteVolumeSell { get; set; }

        [JsonProperty("CURRENT_HOUR_QUOTE_VOLUME_UNKNOWN")]
        public decimal CurrentHourQuoteVolumeUnknown { get; set; }

        [JsonProperty("CURRENT_HOUR_OPEN")]
        public decimal CurrentHourOpen { get; set; }

        [JsonProperty("CURRENT_HOUR_HIGH")]
        public decimal CurrentHourHigh { get; set; }

        [JsonProperty("CURRENT_HOUR_LOW")]
        public decimal CurrentHourLow { get; set; }

        [JsonProperty("CURRENT_HOUR_TOTAL_TRADES")]
        public int CurrentHourTotalTrades { get; set; }

        [JsonProperty("CURRENT_HOUR_TOTAL_TRADES_BUY")]
        public int CurrentHourTotalTradesBuy { get; set; }

        [JsonProperty("CURRENT_HOUR_TOTAL_TRADES_SELL")]
        public int CurrentHourTotalTradesSell { get; set; }

        [JsonProperty("CURRENT_HOUR_TOTAL_TRADES_UNKNOWN")]
        public int CurrentHourTotalTradesUnknown { get; set; }

        [JsonProperty("CURRENT_HOUR_CHANGE")]
        public decimal CurrentHourChange { get; set; }

        [JsonProperty("CURRENT_HOUR_CHANGE_PERCENTAGE")]
        public decimal CurrentHourChangePercentage { get; set; }

        [JsonProperty("PRICE")]
        public decimal Price { get; set; }

        [JsonProperty("PRICE_FLAG")]
        public string PriceFlag { get; set; }

        [JsonProperty("PRICE_LAST_UPDATE_TS")]
        public long PriceLastUpdateTimestamp { get; set; }

        [JsonProperty("PRICE_LAST_UPDATE_TS_NS")]
        public long PriceLastUpdateTimestampNs { get; set; }

        [JsonProperty("CURRENT_DAY_VOLUME")]
        public decimal CurrentDayVolume { get; set; }
    }
}
