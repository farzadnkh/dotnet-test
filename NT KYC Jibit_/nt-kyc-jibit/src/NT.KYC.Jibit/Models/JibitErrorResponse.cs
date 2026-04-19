using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models;

public class JibitErrorResponse
{
    [JsonProperty(PropertyName = "code", NullValueHandling = NullValueHandling.Ignore)]
    public string Code { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
    public string Message { get; set; } = string.Empty;
}