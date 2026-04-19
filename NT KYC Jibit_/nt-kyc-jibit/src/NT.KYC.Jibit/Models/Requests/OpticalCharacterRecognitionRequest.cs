using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Requests;

public class OpticalCharacterRecognitionRequest : IRequestBody
{
    /// <summary>
    /// Jpg or Png image file uploaded by users with maximum 
    /// size of 500kb (national card image)
    /// </summary>
    [JsonProperty(PropertyName = "file")]
    public required IFormFile File { get; set; }

    /// <summary>
    /// This variable is optional and specifies whether OCR should 
    /// be performed on the back of the ID card. Set its value to 
    /// "1" to indicate that the service should process the image of 
    /// the ID card's back.
    /// </summary>
    [JsonProperty(PropertyName = "is_back", DefaultValueHandling = 0)]
    public int? IsBack { get; set; } = 0;
}