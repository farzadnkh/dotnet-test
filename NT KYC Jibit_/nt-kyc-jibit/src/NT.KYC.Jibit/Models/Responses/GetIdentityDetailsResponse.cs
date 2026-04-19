using Newtonsoft.Json;
using NT.KYC.Jibit.Models.Responses.ResponseModels;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class GetIdentityDetailsResponse : IResponseBody
{
    [JsonProperty(PropertyName = "nationalCode")]
    public string NationalCode { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "birthDate")]
    public string BirthDate { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "identityInfo")]
    public JibitIdentityInfo IdentityInfo { get; set; } = new();
}