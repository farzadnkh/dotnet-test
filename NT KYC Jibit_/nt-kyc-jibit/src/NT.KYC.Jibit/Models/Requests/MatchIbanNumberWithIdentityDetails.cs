using Newtonsoft.Json;
using NT.SDK.RestClient.Helpers;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Requests;

public class MatchIbanNumberWithIdentityDetails : IRequestBody
{
    [QueryParameter]
    [JsonProperty(PropertyName = "iban")]
    public required string Iban { get; set; }
    
    [QueryParameter]
    [JsonProperty(PropertyName = "nationalCode")]
    public required string NationalCode { get; set; }
    
    /// <summary>
    /// this should be a birthdate without forwarding slashes
    /// YYYYMMDD
    /// </summary>
    [QueryParameter]
    [JsonProperty(PropertyName = "birthDate")]
    public required string BirthDate { get; set; }
}