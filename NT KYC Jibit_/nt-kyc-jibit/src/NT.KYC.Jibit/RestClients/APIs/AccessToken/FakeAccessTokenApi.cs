using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.KYC.Jibit.RestClients.APIs.AccessToken;

public class FakeJibitAccessTokenApi(JibitRestClientConfiguration configuration, ILogger<FakeJibitAccessTokenApi> logger) : JibitBaseApi(configuration, logger), IJibitAccessTokenApi
{
    public async Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken()
    {
        await Task.Delay(10);

        var fakeResponse = new GetAccessTokenResponse
        {
            AccessToken = "TestToken",
            RefreshToken = "TestRefreshToken"
        };

        var wrapper = new JibitResponseWrapper<GetAccessTokenResponse>
        {
            Data = fakeResponse
        };

        return new ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken(string accessKey, string secretKey)
    {
        await Task.Delay(10);

        var fakeResponse = new GetAccessTokenResponse
        {
            AccessToken = "TestToken",
            RefreshToken = "TestRefreshToken"
        };

        var wrapper = new JibitResponseWrapper<GetAccessTokenResponse>
        {
            Data = fakeResponse
        };

        return new ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> RefreshAccessToken(
        RefreshAccessTokenRequest request)
    {
        await Task.Delay(10);

        var fakeResponse = new GetAccessTokenResponse
        {
            AccessToken = "TestToken",
            RefreshToken = "TestRefreshToken"
        };

        var wrapper = new JibitResponseWrapper<GetAccessTokenResponse>
        {
            Data = fakeResponse
        };

        return new ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>(HttpStatusCode.OK, wrapper);
    }
}