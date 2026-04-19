using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Requests;

public class OneStepKycRequest : JibitBaseRequest
{
    [JsonProperty(PropertyName = "national_id")]
    public string NationalId { get; set; } = string.Empty;

    /// <summary>
    /// Required format YYYY/MM/DD Shamsi date
    /// </summary>
    [JsonProperty(PropertyName = "birth_date")]
    public string BirthDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Either the 'national_card_serial' or the 'birth_date' field is required, but not both.
    /// </summary>
    [JsonProperty(PropertyName = "national_card_serial")]
    public string? NationalCardSerial { get; set; }

    /// <summary>
    /// the video file of user (max 10mb)
    /// The face boundary should cover at least 35% of the input image frame.
    /// The face boundary resolution should be at least 256*256 pixels (considering 
    /// the first requirement, the minimum supported resolution is 1024*768).
    /// </summary>
    [JsonProperty(PropertyName = "video_file", Required = Required.Always )]
    public required IFormFile VideoFile { get; set; }
    
    /// <summary>
    /// Important keywords in the sentence
    /// The keyword must also be at least 6 characters long. (For example, "Blubank" 
    /// includes 7 characters.)
    /// </summary>
    [JsonProperty(PropertyName = "spec_words")]
    public string[]? SpecWords { get; set; }

    /// <summary>
    /// The line that user is required to read in front of camera
    /// The user's template text ("line" parameter) must be at least 60 characters.
    /// </summary>
    [JsonProperty(PropertyName = "line")]
    public string Line { get; set; } = string.Empty;

    /// <summary>
    /// The maximum number of characters allowed to be corrected to match the input word
    /// </summary>
    [JsonProperty(PropertyName = "max_accepted_dist")]
    public int? MaxAcceptedDist { get; set; }

    /// <summary>
    /// Threshold limit for matching or not matching The distance between the said sentence and the template, client defined
    /// </summary>
    [JsonProperty(PropertyName = "threshold")]
    public int? Threshold { get; set; }
}