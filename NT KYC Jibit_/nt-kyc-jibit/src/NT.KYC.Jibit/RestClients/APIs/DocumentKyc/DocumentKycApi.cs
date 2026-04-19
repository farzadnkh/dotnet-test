using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs.DocumentKyc;

public interface IDocumentKycApi
{
    /// <summary>
    /// This API is designed to check whether the user’s uploaded document is the same one as 
    /// expected or not. The default state is currently configured to recognize national cards.(Kart Melli).
    /// the distance between camera and document needs to be close in order to jibit accepts it.
    /// </summary>
    /// <returns>ApiResponse[JibitResponseWrapper[DocumentVerificationResponse]]</returns>
    public Task<ApiResponse<JibitAlphaResponseWrapper<DocumentVerificationResponse>>> GetDocumentVerificationResult(
        DocumentVerificationRequest request, string bearerToken);

    /// <summary>
    /// This API is designed to do OCR on selected documents such as national cards. The system 
    /// extracts the texts in the document and them. The following data is for national card (Kart Melli)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns>ApiResponse[JibitResponseWrapper[OpticalCharacterRecognitionResponse]]</returns>
    public Task<ApiResponse<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>>>
        GetOpticalCharacterRecognitionResult(OpticalCharacterRecognitionRequest request, string bearerToken);
}

public class DocumentKycApi(JibitAlphaRestClientConfiguration configuration, ILogger<DocumentKycApi> logger) : JibitBaseApi(configuration, logger), IDocumentKycApi
{
    public async Task<ApiResponse<JibitAlphaResponseWrapper<DocumentVerificationResponse>>> GetDocumentVerificationResult(
        DocumentVerificationRequest request, string bearerToken)
    {
        var options = RequestOptions<DocumentVerificationRequest>.CreateDefault();

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);
        options.HeaderParameters.Add("Content-Type", "multipart/form-data");
        options.HeaderParameters.Add("Accept", "application/json");

        options.FileParameters.Add(DocumentKycApiConstants.Ssc, request.Ssc.OpenReadStream());

        var response =
            await PostAsync<JibitAlphaResponseWrapper<DocumentVerificationResponse>, DocumentVerificationRequest>(
                JibitAlphaEndPoints.DocumentVerification, options);

        response.Result.AddError(response);
        
        return response;
    }

    public async Task<ApiResponse<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>>>
        GetOpticalCharacterRecognitionResult(OpticalCharacterRecognitionRequest request, string bearerToken)
    {
        var options = RequestOptions<OpticalCharacterRecognitionRequest>.CreateDefault();

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);
        options.HeaderParameters.Add("Content-Type", "multipart/form-data");
        options.HeaderParameters.Add("Accept", "application/json");

        options.FileParameters.Add(DocumentKycApiConstants.File, request.File.OpenReadStream());
        options.FormParameters.Add(DocumentKycApiConstants.IsBack, request.IsBack.ToString());

        var response =
            await PostAsync<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>, OpticalCharacterRecognitionRequest>(
                JibitAlphaEndPoints.OpticalCharacterRecognition, options);

        response.Result.AddError(response);
        
        return response;
    }
}