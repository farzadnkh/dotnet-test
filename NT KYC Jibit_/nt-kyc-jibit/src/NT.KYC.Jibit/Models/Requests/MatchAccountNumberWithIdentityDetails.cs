using Newtonsoft.Json;
using NT.SDK.RestClient.Helpers;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Requests;

public class MatchAccountNumberWithIdentityDetails : IRequestBody
{
    [QueryParameter]
    [JsonProperty(PropertyName = "accountNumber")]
    public required string AccountNumber { get; set; }
    
    /// <summary>
    /// name of the bank should be all capital letter like RESALAT or PASARGAD
    /// </summary>
    [QueryParameter]
    [JsonProperty(PropertyName = "bank")]
    public required string Bank { get; set; }
    
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