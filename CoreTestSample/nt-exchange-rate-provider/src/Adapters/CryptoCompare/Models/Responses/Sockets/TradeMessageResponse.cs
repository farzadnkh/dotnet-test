using Newtonsoft.Json;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Models.Responses.Sockets
{
    public class TradeMessageResponse : InboundMessageBase
    {
        [JsonProperty("MARKET")]
        public string Market { get; set; }

        [JsonProperty("INSTRUMENT")]
        public string Instrument { get; set; }

        [JsonProperty("MAPPED_INSTRUMENT")]
        public string MappedInstrument { get; set; }

        [JsonProperty("BASE")]
        public string BaseAsset { get; set; } 

        [JsonProperty("QUOTE")]
        public string QuoteAsset { get; set; }

        [JsonProperty("BASE_ID")]
        public int BaseId { get; set; }

        [JsonProperty("QUOTE_ID")]
        public int QuoteId { get; set; }

        [JsonProperty("TRANSFORM_FUNCTION")]
        public string TransformFunction { get; set; }

        [JsonProperty("SIDE")]
        public string Side { get; set; } 

        [JsonProperty("ID")]
        public string Id { get; set; } 

        [JsonProperty("TIMESTAMP")]
        public long Timestamp { get; set; } 

        [JsonProperty("TIMESTAMP_NS")]
        public long TimestampNs { get; set; } 

        [JsonProperty("RECEIVED_TIMESTAMP")]
        public long ReceivedTimestamp { get; set; }

        [JsonProperty("RECEIVED_TIMESTAMP_NS")]
        public long ReceivedTimestampNs { get; set; }

        [JsonProperty("QUANTITY")]
        public decimal Quantity { get; set; }

        [JsonProperty("PRICE")]
        public decimal Price { get; set; }

        [JsonProperty("QUOTE_QUANTITY")]
        public decimal QuoteQuantity { get; set; }

        [JsonProperty("SOURCE")]
        public string Source { get; set; }

        [JsonProperty("CCSEQ")]
        public long Ccseq { get; set; }
    }
}
