using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.KYC.Jibit.RestClients.APIs.BiometricKyc;

public class FakeBiometricKycApi(JibitAlphaRestClientConfiguration configuration, ILogger<FakeBiometricKycApi> logger) : JibitBaseApi(configuration, logger), IBiometricKycApi
{
    
    public async Task<ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>> GetOneStepKycResult(OneStepKycRequest request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new OneStepKycResponse();
        var wrapper = new JibitAlphaResponseWrapper<OneStepKycResponse>
        {
            Data = new List<OneStepKycResponse>{response}
        };

        return new ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>>> HealthCheck(string bearerToken)
    {
        await Task.Delay(10);
        
        var response = new JibitAlphaHealthCheckResponse();
        
        var wrapper = new JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>
        {
            Data = new List<JibitAlphaHealthCheckResponse>{response}
        };
        return new ApiResponse<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>>(HttpStatusCode.OK, wrapper);
    }
}