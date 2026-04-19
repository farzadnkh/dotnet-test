using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class LivenessStepResult
{
    [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Ignore)]
    public bool Result { get; set; }
    
    [JsonProperty(PropertyName = "score", NullValueHandling = NullValueHandling.Ignore)]
    public string? Score { get; set; }


}