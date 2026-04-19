using Newtonsoft.Json;
using NT.SDK.RestClient.Helpers;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Requests;


public class MatchMobileNumberWithNationalCode : IRequestBody
{
    [QueryParameter]
    [JsonProperty(PropertyName = "mobileNumber")]
    public required string MobileNumber { get; set; }
    
    [QueryParameter]
    [JsonProperty(PropertyName = "nationalCode")]
    public required string NationalCode { get; set; }
}