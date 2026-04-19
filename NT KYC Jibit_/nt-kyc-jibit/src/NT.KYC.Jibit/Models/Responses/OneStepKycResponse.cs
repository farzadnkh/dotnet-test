using Newtonsoft.Json;
using NT.KYC.Jibit.Models.Responses.ResponseModels;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class OneStepKycResponse : IResponseBody
{
    /// <summary>
    /// only check for status of asr result 
    /// this property shows summation of 
    /// kyc result was successful or failed
    /// true for success and false for 
    /// failed result
    /// </summary>
    [JsonProperty(PropertyName = "status")]
    public bool Status { get; set; }
    
    /// <summary>
    /// Contains data about the ASR result
    /// </summary>
    [JsonProperty(PropertyName = "asr", NullValueHandling = NullValueHandling.Ignore)]
    public JibitAsrData? Asr { get; set; }
    
    /// <summary>
    /// shows the result of liveness
    /// </summary>
    [JsonProperty(PropertyName = "liveness", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, LivenessStepResult>? Liveness { get; set; }
    
    /// <summary>
    /// shows the result of verification
    /// </summary>
    [JsonProperty(PropertyName = "verification", NullValueHandling = NullValueHandling.Ignore)]
    public OneStepVerificationStepResult? Verification { get; set; }
}