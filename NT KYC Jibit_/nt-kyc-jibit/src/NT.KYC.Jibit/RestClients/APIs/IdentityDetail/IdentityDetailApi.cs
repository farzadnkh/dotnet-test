using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs.IdentityDetail;

public interface IIdentityDetailApi
{
    /// <summary>
    ///     use this Api to get user identity details from Sabt Ahval
    /// </summary>
    /// <param name="request">GetIdentityDetailsRequest</param>
    /// <param name="bearerToken">string</param>
    /// <returns>ApiResponse[JibitResponseWrapper[GetIdentityDetailsResponse]]</returns>
    public Task<ApiResponse<JibitResponseWrapper<GetIdentityDetailsResponse>>> GetIdentityDetails(
        GetIdentityDetailsRequest request, string bearerToken);
}

public class IdentityDetailApi(JibitRestClientConfiguration configuration, ILogger<IdentityDetailApi> logger) : JibitBaseApi(configuration, logger), IIdentityDetailApi
{
    public async Task<ApiResponse<JibitResponseWrapper<GetIdentityDetailsResponse>>> GetIdentityDetails(
        GetIdentityDetailsRequest request,
        string bearerToken)
    {
        var options = RequestOptions<GetIdentityDetailsRequest>.CreateDefault();

        options.SetBody(request);

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);
        
        options.QueryParameters.Add(IdentityDetailApiConstants.BirthDate, request.BirthDate);
        options.QueryParameters.Add(IdentityDetailApiConstants.NationalCode, request.NationalCode);

        var result =
            await GetAsync<JibitResponseWrapper<GetIdentityDetailsResponse>>(JibitEndPoints.GetIdentityDetails,
                options);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }
}