using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Requests;

public class RefreshAccessTokenRequest : JibitBaseRequest
{
    [JsonProperty(PropertyName = "accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
}