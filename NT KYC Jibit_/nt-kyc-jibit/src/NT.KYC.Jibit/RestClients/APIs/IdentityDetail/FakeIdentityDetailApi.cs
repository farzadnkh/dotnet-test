using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.KYC.Jibit.RestClients.APIs.IdentityDetail;

public class FakeIdentityDetailApi(JibitRestClientConfiguration configuration, ILogger<FakeIdentityDetailApi> logger) : JibitBaseApi(configuration, logger), IIdentityDetailApi
{
    public async Task<ApiResponse<JibitResponseWrapper<GetIdentityDetailsResponse>>> GetIdentityDetails(
        GetIdentityDetailsRequest request,
        string bearerToken)
    {
        await Task.Delay(10);

        var response = new GetIdentityDetailsResponse
        {
            BirthDate = "1378/07/10",
            NationalCode = "1362262266"
        };

        var wrapper = new JibitResponseWrapper<GetIdentityDetailsResponse>
        {
            Data = response
        };

        return new ApiResponse<JibitResponseWrapper<GetIdentityDetailsResponse>>(HttpStatusCode.OK, wrapper);
    }
}