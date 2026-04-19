using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class OpticalCharacterRecognitionResponse : IResponseBody
{
    [JsonProperty(PropertyName = "best_accuracy", NullValueHandling = NullValueHandling.Ignore)]
    public float? BestAccuracy { get; set; }

    [JsonProperty(PropertyName = "accuracy", NullValueHandling = NullValueHandling.Ignore)]
    public float? Accuracy { get; set; }
    
    [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
    public string? Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Social security number
    /// </summary>
    [JsonProperty(PropertyName = "id_num")]
    public string? IdNumber { get; set; }
    
    /// <summary>
    /// The name field in the national card
    /// </summary>
    [JsonProperty(PropertyName = "name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The family name in the national card
    /// </summary>
    [JsonProperty(PropertyName = "family_name")]
    public string? FamilyName { get; set; }
    
    /// <summary>
    /// The birth data in the national card
    /// </summary>
    [JsonProperty(PropertyName = "birth_date")]
    public string? BirthDate { get; set; }
    
    /// <summary>
    /// The father’s name in the national card
    /// </summary>
    [JsonProperty(PropertyName = "father_name")]
    public string? FatherName { get; set; }
    
    /// <summary>
    /// The expiration data in the national card
    /// </summary>
    [JsonProperty(PropertyName = "expiration_date")]
    public string? ExpirationDate { get; set; }

    /// <summary>
    /// The social security card code
    /// </summary>
    [JsonProperty(PropertyName = "code", NullValueHandling = NullValueHandling.Ignore)]
    public string Code { get; set; } = string.Empty;
}