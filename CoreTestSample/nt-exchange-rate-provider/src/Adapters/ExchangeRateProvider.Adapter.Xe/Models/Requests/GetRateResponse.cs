using Newtonsoft.Json;

namespace ExchangeRateProvider.Adapter.Xe.Models.Requests;

public class GetRateResponse
{
    [JsonProperty("terms")]
    public string Terms { get; set; }

    [JsonProperty("privacy")]
    public string Privacy { get; set; }

    [JsonProperty("from")]
    public string From { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("to")]
    public List<To> To { get; set; }
}

public class To
{
    [JsonProperty("quotecurrency")]
    public string Quotecurrency { get; set; }

    [JsonProperty("mid")]
    public decimal Mid { get; set; }
}
