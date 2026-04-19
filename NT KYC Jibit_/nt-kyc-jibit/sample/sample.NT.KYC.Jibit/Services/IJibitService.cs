using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;

namespace sample.NT.KYC.Jibit.Services;

public interface IJibitService
{
    Task<JibitResponseWrapper<GetIdentityDetailsResponse>> GetIdentityDetails(GetIdentityDetailsRequest request);

    Task<JibitAlphaResponseWrapper<OneStepKycResponse>> GetOneStepKycData(OneStepKycRequest request);

    Task<JibitAlphaResponseWrapper<DocumentVerificationResponse>> GetDocumentVerificationResult(DocumentVerificationRequest request);
    Task<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>> GetOpticalCharacterRecognitionResult(OpticalCharacterRecognitionRequest request);

    Task<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>> HealthCheck();

    Task<JibitResponseWrapper<JibitMatchResult>> MatchCardNumberWithIdentityDetails(
        MatchCardNumberWithIdentityDetails request);

    Task<JibitResponseWrapper<JibitMatchResult>> MatchMobileNumberWithNationalCode(
        MatchMobileNumberWithNationalCode request);

    public Task<JibitResponseWrapper<JibitMatchResult>> MatchAccountNumberWithIdentityDetails(
        MatchAccountNumberWithIdentityDetails request);

    public Task<JibitResponseWrapper<JibitMatchResult>> MatchIbanWithIdentityDetails(
        MatchIbanNumberWithIdentityDetails request);

    public Task<JibitResponseWrapper<CardNumberToIbanResponse>> CardNumberToIban(
        CardNumberToIbanRequest request);
    
    public Task<JibitResponseWrapper<GetAccessTokenResponse>> GetToken();
    
    public Task<JibitResponseWrapper<GetAccessTokenResponse>> RefreshToken(
        RefreshAccessTokenRequest request);
}