using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.KYC.Jibit.RestClients.APIs.Matching;

public class FakeMatchingApi(JibitRestClientConfiguration configuration, ILogger<FakeMatchingApi> logger) : JibitBaseApi(configuration, logger), IMatchingApi
{
    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchMobileNumberWithNationalCode(MatchMobileNumberWithNationalCode request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new JibitMatchResult
        {
            Matched = true
        };

        var wrapper = new JibitResponseWrapper<JibitMatchResult>
        {
            Data = response
        };

        return new ApiResponse<JibitResponseWrapper<JibitMatchResult>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchCardNumberWithIdentityDetails(MatchCardNumberWithIdentityDetails request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new JibitMatchResult
        {
            Matched = true
        };

        var wrapper = new JibitResponseWrapper<JibitMatchResult>
        {
            Data = response
        };

        return new ApiResponse<JibitResponseWrapper<JibitMatchResult>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchIbanWithIdentityDetails(MatchIbanNumberWithIdentityDetails request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new JibitMatchResult
        {
            Matched = true
        };

        var wrapper = new JibitResponseWrapper<JibitMatchResult>
        {
            Data = response
        };

        return new ApiResponse<JibitResponseWrapper<JibitMatchResult>>(HttpStatusCode.OK, wrapper);
    }

    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchAccountNumberWithIdentityDetails(MatchAccountNumberWithIdentityDetails request, string bearerToken)
    {
        await Task.Delay(10);

        var response = new JibitMatchResult
        {
            Matched = true
        };

        var wrapper = new JibitResponseWrapper<JibitMatchResult>
        {
            Data = response
        };

        return new ApiResponse<JibitResponseWrapper<JibitMatchResult>>(HttpStatusCode.OK, wrapper);
    }
}