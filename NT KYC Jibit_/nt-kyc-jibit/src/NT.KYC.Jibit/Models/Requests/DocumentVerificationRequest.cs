using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Requests;

public class DocumentVerificationRequest : IRequestBody
{
    /// <summary>
    /// Maximum Size 500 kb.
    /// </summary>
    [JsonProperty(PropertyName = "ssc")] public required IFormFile Ssc { get; set; }
}