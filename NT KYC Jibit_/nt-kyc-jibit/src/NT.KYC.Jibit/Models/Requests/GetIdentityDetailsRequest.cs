using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NT.SDK.RestClient.Helpers;

namespace NT.KYC.Jibit.Models.Requests;

public class GetIdentityDetailsRequest : JibitBaseRequest
{
    [Required]
    [QueryParameter]
    [JsonIgnore]
    public string NationalCode { get; set; } = string.Empty;

    [Required]
    [QueryParameter]
    [JsonIgnore]
    public string BirthDate { get; set; } = string.Empty;
}