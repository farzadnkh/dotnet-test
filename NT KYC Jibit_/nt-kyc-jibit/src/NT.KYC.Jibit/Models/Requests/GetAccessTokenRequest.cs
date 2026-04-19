using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Requests;

public class GetAccessTokenRequest : JibitBaseRequest
{
    [JsonProperty(PropertyName = "apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "secretKey")]
    public string SecretKey { get; set; } = string.Empty;
}