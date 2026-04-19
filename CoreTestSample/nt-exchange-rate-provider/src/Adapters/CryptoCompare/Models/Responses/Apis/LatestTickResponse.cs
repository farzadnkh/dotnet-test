using System.Text.Json.Serialization;
using NT.SDK.RestClient.Models;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Models.Responses.Apis;

public class LatestTickResponse : IResponseBody
{
    [JsonPropertyName("data")]
    public Dictionary<string, LatestTickItem> Data { get; set; } = [];

    [JsonPropertyName("err")]
    public LatestTickError Err { get; set; }
}

public class LatestTickError
{
}


public class LatestTickItem
{
    /// <summary>
    /// Type of the data feed.
    /// </summary>
    [JsonPropertyName("TYPE")]
    public string Type { get; set; }

    /// <summary>
    /// Market where the instrument is traded (e.g., coinbase).
    /// </summary>
    [JsonPropertyName("MARKET")]
    public string Market { get; set; }

    /// <summary>
    /// Trading instrument (e.g., BTC-USD).
    /// </summary>
    [JsonPropertyName("INSTRUMENT")]
    public string Instrument { get; set; }

    /// <summary>
    /// Cryptocurrency sequence number.
    /// </summary>
    [JsonPropertyName("CCSEQ")]
    public long Ccseq { get; set; }

    /// <summary>
    /// Current price of the instrument.
    /// </summary>
    [JsonPropertyName("PRICE")]
    public decimal Price { get; set; }

    /// <summary>
    /// Flag indicating the price movement (e.g., UP, DOWN, UNCHANGED).
    /// </summary>
    [JsonPropertyName("PRICE_FLAG")]
    public string PriceFlag { get; set; }

    /// <summary>
    /// Timestamp of the last price update in Unix milliseconds.
    /// </summary>
    [JsonPropertyName("PRICE_LAST_UPDATE_TS")]
    public long PriceLastUpdateTs { get; set; }

    /// <summary>
    /// Nanoseconds part of the last price update timestamp.
    /// </summary>
    [JsonPropertyName("PRICE_LAST_UPDATE_TS_NS")]
    public int PriceLastUpdateTsNs { get; set; }

    [JsonPropertyName("CURRENT_DAY_VOLUME")]
    public decimal CurrentDayVolume { get; set; }
}