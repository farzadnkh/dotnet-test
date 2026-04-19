using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class OneStepVerificationStepResult
{
    [JsonProperty(PropertyName = "results", NullValueHandling = NullValueHandling.Ignore)]
    public List<bool>? Results { get; set; }

    [JsonProperty(PropertyName = "details", NullValueHandling = NullValueHandling.Ignore)]
    public List<OneStepVerificationStepResultDetails>? Details { get; set; }

    [JsonProperty(PropertyName = "duration", NullValueHandling = NullValueHandling.Ignore)]
    public float? Duration { get; set; }

    [JsonProperty(PropertyName = "true_rate", NullValueHandling = NullValueHandling.Ignore)]
    public float? TrueRate { get; set; }
    
    [JsonProperty(PropertyName = "total_similarity", NullValueHandling = NullValueHandling.Ignore)]
    public float? TotalSimilarity { get; set; }
    
}

public class OneStepVerificationStepResultDetails
{
    [JsonProperty(PropertyName = "distance", NullValueHandling = NullValueHandling.Ignore)]
    public float? Distance { get; set; }

    [JsonProperty(PropertyName = "similarity", NullValueHandling = NullValueHandling.Ignore)]
    public float? Similarity { get; set; }
}

public class OneStepVerificationStepResultStatuse
{
    [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Status { get; set; }

    [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
    public string? Message { get; set; }
}