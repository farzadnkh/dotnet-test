using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.KYC.Jibit.RestClients.APIs.DocumentKyc;

public class FakeDocumentKycApi(JibitAlphaRestClientConfiguration configuration, ILogger<FakeDocumentKycApi> logger) : JibitBaseApi(configuration, logger), IDocumentKycApi
{
    public async Task<ApiResponse<JibitAlphaResponseWrapper<DocumentVerificationResponse>>> GetDocumentVerificationResult(DocumentVerificationRequest request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new DocumentVerificationResponse();
        var wrapper = new JibitAlphaResponseWrapper<DocumentVerificationResponse>
        {
            Data = new List<DocumentVerificationResponse>{response}
        };

        return new ApiResponse<JibitAlphaResponseWrapper<DocumentVerificationResponse>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>>> GetOpticalCharacterRecognitionResult(OpticalCharacterRecognitionRequest request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new OpticalCharacterRecognitionResponse();
        var wrapper = new JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>
        {
            Data = new List<OpticalCharacterRecognitionResponse>{response}
        };

        return new ApiResponse<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>>(HttpStatusCode.OK, wrapper);
    }
}