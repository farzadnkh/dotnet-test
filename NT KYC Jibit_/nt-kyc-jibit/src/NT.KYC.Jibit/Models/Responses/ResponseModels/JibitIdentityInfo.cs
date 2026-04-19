using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class JibitIdentityInfo
{
    [JsonProperty(PropertyName = "nationalCode")]
    public string NationalCode { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "fatherName")]
    public string FatherName { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "birthDate")]
    public string BirthDate { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "alive")] public bool Alive { get; set; }

    /// <summary>
    ///     MALE, FEMALE, UNKNOWN
    /// </summary>
    [JsonProperty(PropertyName = "gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "identificationNumber")]
    public int IdentificationNumber { get; set; }

    [JsonProperty(PropertyName = "identificationSerialCode")]
    public string IdentificationSerialCode { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "identificationSerialNumber")]
    public int IdentificationSerialNumber { get; set; }

    [JsonProperty(PropertyName = "providerTrackerId")]
    public string ProviderTrackerId { get; set; } = string.Empty;
}