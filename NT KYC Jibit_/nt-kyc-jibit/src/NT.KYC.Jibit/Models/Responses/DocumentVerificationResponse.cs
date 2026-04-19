using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class DocumentVerificationResponse : IResponseBody
{
    /// <summary>
    /// a message that explains the rejection. 
    /// </summary>
    [JsonProperty(PropertyName = "message")]
    public string? Message { get; set; }

    /// <summary>
    /// Whether the uploaded 
    /// document is the supposed one.
    /// </summary>
    [JsonProperty(PropertyName = "detected")]
    public bool Detected { get; set; }
}