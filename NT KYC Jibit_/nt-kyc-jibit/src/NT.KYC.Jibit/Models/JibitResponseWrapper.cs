using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models;

public class JibitResponseWrapper<T> : IResponseWrapper where T : IResponseBody
{
    public T? Data { get; set; }

    public ICollection<ApisErrorResponseData>? Errors { get; set; }

    public void AddJibitErrorResponse(ApiResponse<JibitResponseWrapper<T>> response)
    {
        if ((int)response.StatusCode < 400 || response.Result.Data != null) return;
        var error = JsonConvert.DeserializeObject<JibitErrorResponse>(response.RawContent);
        if (error != null) AddError(error);
    }

    public void TransferToObject(string stringJson)
    {
        Data = JsonConvert.DeserializeObject<T>(stringJson);
    }
    
    private void AddError(JibitErrorResponse error)
    {
        Errors ??= new List<ApisErrorResponseData>();

        Errors.Add(new ApisErrorResponseData
        {
            Code = error.Code,
            Message = error.Message
        });
    }
}