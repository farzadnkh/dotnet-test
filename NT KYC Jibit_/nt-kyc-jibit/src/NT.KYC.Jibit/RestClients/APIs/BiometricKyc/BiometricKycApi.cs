using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs.BiometricKyc;

public interface IBiometricKycApi
{
    /// <summary>
    /// This API designed to check whether the user's speech matched with the content (Text) you intended or not.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>> GetOneStepKycResult(
        OneStepKycRequest request,
        string bearerToken);

    public Task<ApiResponse<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>>> HealthCheck(string bearerToken);
}

public class BiometricKycApi(JibitAlphaRestClientConfiguration configuration, ILogger<BiometricKycApi> logger) : JibitBaseApi(configuration, logger), IBiometricKycApi
{
    public async Task<ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>> GetOneStepKycResult(
        OneStepKycRequest request, string bearerToken)
    {
        var options = RequestOptions<OneStepKycRequest>.CreateDefault();

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);
        options.HeaderParameters.Add("Content-Type", "multipart/form-data");
        options.HeaderParameters.Add("Accept", "application/json");

        PrepareOneStepKycForm(request, options);

        var response = await PostAsync
            <JibitAlphaResponseWrapper<OneStepKycResponse>, OneStepKycRequest>(
                JibitAlphaEndPoints.OneStepKycApi, options);

        response.Result.AddError(response);

        return response;
    }

    public async Task<ApiResponse<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>>> HealthCheck(
        string bearerToken)
    {
        var options = RequestOptions.CreateDefault();

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);

        var result =
            await GetAsync<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>>(JibitAlphaEndPoints.HealthCheck,
                options);

        result.Result.AddError(result);
        
        return result;
    }

    private static void PrepareOneStepKycForm(OneStepKycRequest request, RequestOptions options)
    {
        options.FileParameters.Add(BiometricKycApiConstants.VideoFile, request.VideoFile.OpenReadStream());

        options.FormParameters.Add(BiometricKycApiConstants.NationalId, request.NationalId);
        options.FormParameters.Add(BiometricKycApiConstants.BirthDate, request.BirthDate);
        options.FormParameters.Add(BiometricKycApiConstants.Line, request.Line);

        if (request.NationalCardSerial != null)
            options.FormParameters.Add(BiometricKycApiConstants.NationalCardSerial, request.NationalCardSerial);
        if (request.SpecWords != null)
            options.FormParameters.Add(BiometricKycApiConstants.SpecWords, string.Join(",", request.SpecWords));
        if (request.MaxAcceptedDist != null)
            options.FormParameters.Add(BiometricKycApiConstants.MaxAcceptedDist, request.MaxAcceptedDist.ToString());
        if (request.Threshold != null)
            options.FormParameters.Add(BiometricKycApiConstants.Threshold, request.Threshold.ToString());
    }
}