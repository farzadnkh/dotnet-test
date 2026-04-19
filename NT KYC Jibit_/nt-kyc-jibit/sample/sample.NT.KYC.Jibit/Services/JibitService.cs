using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.RestClients;
using NT.SDK.RestClient.Logger;
using NT.SDK.RestClient.Models;

namespace sample.NT.KYC.Jibit.Services;

public class JibitService(
    IJibitRestClient jibitRestClient,
    IJibitAlphaRestClient jibitAlphaRestClient,
    JibitCredentialsManagement jibitCredentialsManagement)
    : IJibitService
{
    private JibitCredentialsCacheDto? credentials = jibitCredentialsManagement.GetCredentialsFromCache();

    public async Task<JibitResponseWrapper<GetIdentityDetailsResponse>> GetIdentityDetails(
        GetIdentityDetailsRequest request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.IdentityDetailApi.GetIdentityDetails(request, credentials!.BearerToken));
    }

    public async Task<JibitResponseWrapper<JibitMatchResult>> MatchCardNumberWithIdentityDetails(
        MatchCardNumberWithIdentityDetails request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.MatchingApi.MatchCardNumberWithIdentityDetails(request, credentials!.BearerToken));
    }
    
    public async Task<JibitResponseWrapper<JibitMatchResult>> MatchAccountNumberWithIdentityDetails(
        MatchAccountNumberWithIdentityDetails request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.MatchingApi.MatchAccountNumberWithIdentityDetails(request, credentials!.BearerToken));
    }
    
    public async Task<JibitResponseWrapper<JibitMatchResult>> MatchIbanWithIdentityDetails(
        MatchIbanNumberWithIdentityDetails request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.MatchingApi.MatchIbanWithIdentityDetails(request, credentials!.BearerToken));
    }
    
    public async Task<JibitResponseWrapper<CardNumberToIbanResponse>> CardNumberToIban(
        CardNumberToIbanRequest request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.CardsApi.CardNumberToIban(request, credentials!.BearerToken));
    }

    public async Task<JibitResponseWrapper<GetAccessTokenResponse>> GetToken()
    {
        var result = await jibitRestClient.JibitAccessTokenApi.GetAccessToken();
        return result.Result;
    }

    public async Task<JibitResponseWrapper<GetAccessTokenResponse>> RefreshToken(RefreshAccessTokenRequest request)
    {
        var result = await jibitRestClient.JibitAccessTokenApi.RefreshAccessToken(request);
        return result.Result;
    }

    public async Task<JibitResponseWrapper<JibitMatchResult>> MatchMobileNumberWithNationalCode(
        MatchMobileNumberWithNationalCode request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.MatchingApi.MatchMobileNumberWithNationalCode(request, credentials!.BearerToken));
    }
    public async Task<JibitAlphaResponseWrapper<OneStepKycResponse>> GetOneStepKycData(
        OneStepKycRequest request)
    {
        return await HandleRequestWithoutCredentials(() =>
            jibitAlphaRestClient.BiometricKycApi.GetOneStepKycResult(request, credentials!.PermanentToken));
    }

    public async Task<JibitAlphaResponseWrapper<DocumentVerificationResponse>> GetDocumentVerificationResult(
        DocumentVerificationRequest request)
    {
        return await HandleRequestWithoutCredentials(() =>
            jibitAlphaRestClient.DocumentKycApi.GetDocumentVerificationResult(request, credentials!.PermanentToken));
    }

    public async Task<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>>
        GetOpticalCharacterRecognitionResult(OpticalCharacterRecognitionRequest request)
    {
        return await HandleRequestWithoutCredentials(() =>
            jibitAlphaRestClient.DocumentKycApi.GetOpticalCharacterRecognitionResult(request,
                credentials!.PermanentToken));
    }

    public async Task<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>> HealthCheck()
    {
        return await HandleRequestWithoutCredentials(() =>
            jibitAlphaRestClient.BiometricKycApi.HealthCheck(credentials!.PermanentToken));
    }

    private async Task<JibitResponseWrapper<TResponse>> HandleRequestWithCredentials<TResponse>(
        Func<Task<ApiResponse<JibitResponseWrapper<TResponse>>>> apiCall)
        where TResponse : IResponseBody
    {
        if (credentials == null) await GetNewJibitCredentials();

        ArgumentNullException.ThrowIfNull(credentials);

        var response = await apiCall();
        if (!response.Successed && response.Result?.Errors?.First().Code == "forbidden")
        {
            await RefreshJibitCredentials();
            response = await apiCall();
        }

        ArgumentNullException.ThrowIfNull(response.Result);

        return response.Result;
    }

    private async Task<JibitAlphaResponseWrapper<TResponse>> HandleRequestWithoutCredentials<TResponse>(
        Func<Task<ApiResponse<JibitAlphaResponseWrapper<TResponse>>>> apiCall)
        where TResponse : IResponseBody
    {
        credentials ??= new JibitCredentialsCacheDto()
        {
            PermanentToken = jibitAlphaRestClient.JibitAlphaConfiguration.PermanentToken
        };

        var response = await apiCall();

        return response.Result ?? throw new Exception("The result is empty");
    }

    private async Task GetNewJibitCredentials()
    {
        var result = await GetAccessToken();

        var data = new JibitCredentialsCacheDto
        {
            BearerToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            PermanentToken = jibitAlphaRestClient.JibitAlphaConfiguration.PermanentToken
        };

        jibitCredentialsManagement.StoreNewCredentialsInCache(data);

        credentials = data;
    }

    private async Task RefreshJibitCredentials()
    {
        var request = PrepareRefreshAccessTokenRequest();

        var result = await RefreshAccessToken(request);

        var data = new JibitCredentialsCacheDto
        {
            BearerToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            PermanentToken = jibitAlphaRestClient.JibitAlphaConfiguration.PermanentToken
        };

        jibitCredentialsManagement.StoreNewCredentialsInCache(data);

        credentials = data;
    }

    private RefreshAccessTokenRequest PrepareRefreshAccessTokenRequest()
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(credentials.BearerToken);
        ArgumentNullException.ThrowIfNull(credentials.RefreshToken);

        var request = new RefreshAccessTokenRequest
        {
            AccessToken = credentials.BearerToken,
            RefreshToken = credentials.RefreshToken
        };
        return request;
    }

    private async Task<GetAccessTokenResponse> GetAccessToken()
    {
        var response = await jibitRestClient.JibitAccessTokenApi.GetAccessToken();

        if (!response.Successed)
            throw new Exception(response.RawContent);

        ArgumentNullException.ThrowIfNull(response.Result);
        ArgumentNullException.ThrowIfNull(response.Result.Data);
        return response.Result.Data;
    }

    private async Task<GetAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest request)
    {
        var response = await jibitRestClient.JibitAccessTokenApi.RefreshAccessToken(request);

        if (!response.Successed)
            throw new Exception(response.Result.Errors != null
                ? response.Result.Errors.First().Message
                : "Unknown Error happened");

        ArgumentNullException.ThrowIfNull(response.Result.Data);
        return response.Result.Data;
    }
}
public delegate void ApiLoggerDelegate(ApiLogger apiLogger, ILogger logger);

