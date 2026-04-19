using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class Owner
{
    /// <summary>
    /// The first name of the owner.
    /// </summary>
    [JsonProperty(PropertyName = "firstName")]
    public string? FirstName { get; set; }

    /// <summary>
    /// The last name of the owner.
    /// </summary>
    [JsonProperty(PropertyName = "lastName")]
    public string? LastName { get; set; }
}