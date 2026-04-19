using Newtonsoft.Json;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models;

public class JibitAlphaResponseWrapper<T> : IResponseWrapper where T : IResponseBody
{
    [JsonProperty(PropertyName = "errorCode")]
    public string? ErrorCode { get; set; }

    [JsonProperty(PropertyName = "errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonConverter(typeof(SingleOrArrayConverter))]
    [JsonProperty(PropertyName = "data")] public List<T>? Data { get; set; }

    public ICollection<ApisErrorResponseData>? Errors { get; set; }


    public void AddError(ApiResponse<JibitAlphaResponseWrapper<T>> response)
    {
        if (string.IsNullOrEmpty(response.RawContent) || (int)response.StatusCode < 400) return;

        Errors ??= new List<ApisErrorResponseData>();

        if (!string.IsNullOrEmpty(response.Result.ErrorMessage))
        {
            Errors.Add(new ApisErrorResponseData
            {
                Message = response.Result.ErrorMessage,
                Code = response.Result.ErrorCode
            });
        }
        else
        {
            var code = (int)response.StatusCode;
            Errors.Add(new ApisErrorResponseData
            {
                Message = JsonConvert.DeserializeObject<JibitAlphaErrorResponse>(response.RawContent)?.Message,
                Code = code.ToString(),
            });
        }
    }
}