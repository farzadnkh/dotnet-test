using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class JibitAsrData
{
    [JsonProperty(PropertyName = "distance")]
    public float Distance { get; set; }

    [JsonProperty(PropertyName = "time")] public float Time { get; set; }

    [JsonProperty(PropertyName = "state")] public bool State { get; set; }
    
    [JsonProperty(PropertyName = "similarity", NullValueHandling = NullValueHandling.Ignore)]
    public float? Similarity { get; set; }

    [JsonProperty(PropertyName = "word", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, bool>? Word { get; set; }
}