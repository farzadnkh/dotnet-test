using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class JibitAlphaHealthCheckResponse : IResponseBody
{
    [JsonProperty("mysql")]
    public ServiceStatus? MySql { get; set; }

    [JsonProperty("jibit_sabte_ahval")]
    public ServiceStatus? JibitSabteAhval { get; set; }

    [JsonProperty("liveness")]
    public ServiceStatus? Liveness { get; set; }

    [JsonProperty("liveness_v2")]
    public ServiceStatus? LivenessV2 { get; set; }

    [JsonProperty("liveness_v3")]
    public ServiceStatus? LivenessV3 { get; set; }

    [JsonProperty("verifier")]
    public ServiceStatus? Verifier { get; set; }

    [JsonProperty("verifier_v2")]
    public ServiceStatus? VerifierV2 { get; set; }

    [JsonProperty("verifier_v3")]
    public ServiceStatus? VerifierV3 { get; set; }

    [JsonProperty("asr")]
    public ServiceStatus? Asr { get; set; }

    [JsonProperty("dynamic_asr")]
    public ServiceStatus? DynamicAsr { get; set; }

    [JsonProperty("ocr")]
    public ServiceStatus? Ocr { get; set; }

    [JsonProperty("age_gender")]
    public ServiceStatus? AgeGender { get; set; }

    [JsonProperty("hijab")]
    public ServiceStatus? Hijab { get; set; }

    [JsonProperty("ssc")]
    public ServiceStatus? Ssc { get; set; }
}

public class ServiceStatus
{
    [JsonProperty("ping")]
    public bool Ping { get; set; }

    [JsonProperty("message")] public string Message { get; set; } = string.Empty;
}