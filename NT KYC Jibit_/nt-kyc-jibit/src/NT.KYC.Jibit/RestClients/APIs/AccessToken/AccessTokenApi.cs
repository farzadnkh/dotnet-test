using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs.AccessToken;

public interface IJibitAccessTokenApi
{
    /// <summary>
    ///     Use this api to get access token for using Jibit APIs.
    /// </summary>
    public Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken();
    
    public Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken(string accessKey, string secretKey);

    /// <summary>
    ///     the refresh token itself is valid for 48 hours. use this endpoint to refresh the access token.
    ///     the access token itself is valid 24 hours.
    /// </summary>
    /// <param name="request">RefreshAccessTokenRequest</param>
    public Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> RefreshAccessToken(
        RefreshAccessTokenRequest request);
}

public class JibitAccessTokenApi(JibitRestClientConfiguration configuration, ILogger<JibitAccessTokenApi> logger) : JibitBaseApi(configuration, logger), IJibitAccessTokenApi
{
    public async Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken()
    {
        var request = new GetAccessTokenRequest
        {
            ApiKey = Configuration.ApiKey,
            SecretKey = Configuration.Password
        };
        var options = RequestOptions<GetAccessTokenRequest>.CreateDefault();

        options.SetBody(request);

        var result = await PostAsync<JibitResponseWrapper<GetAccessTokenResponse>, GetAccessTokenRequest>(
            JibitEndPoints.GetAccessToken,
            options).ConfigureAwait(false);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }

    public async Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken(string apikey, string secretKey)
    {
        var request = new GetAccessTokenRequest
        {
            ApiKey = apikey,
            SecretKey = secretKey
        };
        var options = RequestOptions<GetAccessTokenRequest>.CreateDefault();

        options.SetBody(request);

        var result = await PostAsync<JibitResponseWrapper<GetAccessTokenResponse>, GetAccessTokenRequest>(
            JibitEndPoints.GetAccessToken,
            options).ConfigureAwait(false);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }

    public async Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> RefreshAccessToken(
        RefreshAccessTokenRequest request)
    {
        var options = RequestOptions<RefreshAccessTokenRequest>.CreateDefault();

        options.SetBody(request);

        var result = await PostAsync<JibitResponseWrapper<GetAccessTokenResponse>, RefreshAccessTokenRequest>(
            JibitEndPoints.RefreshAccessToken,
            options).ConfigureAwait(false);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }
}