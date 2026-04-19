using System.Text.Json.Serialization;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Models.Responses.Sockets;

public class InboundMessageBase
{
    [JsonPropertyName("TYPE")] public string Type { get; set; }
}
