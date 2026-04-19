using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class GetAccessTokenResponse : IResponseBody
{
    /// <summary>
    ///     Valid for 24 hour
    /// </summary>
    [JsonProperty(PropertyName = "accessToken", NullValueHandling = NullValueHandling.Ignore)]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    ///     Valid for 48 hour
    /// </summary>
    [JsonProperty(PropertyName = "refreshToken", NullValueHandling = NullValueHandling.Ignore)]
    public string RefreshToken { get; set; } = string.Empty;
}