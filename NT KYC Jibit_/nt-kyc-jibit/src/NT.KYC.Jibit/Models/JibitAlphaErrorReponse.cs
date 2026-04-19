using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models;

public class JibitAlphaErrorResponse
{
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; } = string.Empty;
}