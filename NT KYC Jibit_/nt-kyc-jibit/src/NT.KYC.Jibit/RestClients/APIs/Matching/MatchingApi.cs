using Microsoft.Extensions.Logging;
using NT.KYC.Jibit.Models;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.RestClients.APIs.Matching;

public interface IMatchingApi
{
    /// <summary>
    /// this method matches the client mobile number with the national code provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchMobileNumberWithNationalCode(MatchMobileNumberWithNationalCode request, string bearerToken);
    
    /// <summary>
    /// this method matches the client card number with the national code and birthdate provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchCardNumberWithIdentityDetails(MatchCardNumberWithIdentityDetails request, string bearerToken);
    
    /// <summary>
    /// this method matches the client Iban number with the national code and birthdate provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchIbanWithIdentityDetails(MatchIbanNumberWithIdentityDetails request, string bearerToken);
    
    /// <summary>
    /// this method matches the client Account number with the national code and birthdate provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>s
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchAccountNumberWithIdentityDetails(MatchAccountNumberWithIdentityDetails request, string bearerToken);
}

public class MatchingApi(JibitRestClientConfiguration configuration, ILogger<JibitBaseApi> logger) : JibitBaseApi(configuration, logger), IMatchingApi
{
    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchMobileNumberWithNationalCode(MatchMobileNumberWithNationalCode request, string bearerToken)
    {
        var options = RequestOptions<MatchMobileNumberWithNationalCode>.CreateDefault();

        options.SetBody(request);

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);

        options.QueryParameters.Add(MatchingApiConstants.MobileNumber, request.MobileNumber);
        options.QueryParameters.Add(MatchingApiConstants.NationalCode, request.NationalCode);

        var result =
            await GetAsync<JibitResponseWrapper<JibitMatchResult>>(JibitEndPoints.GetMatchingResult,
                options);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }

    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchCardNumberWithIdentityDetails(MatchCardNumberWithIdentityDetails request, string bearerToken)
    {
        var options = RequestOptions<MatchCardNumberWithIdentityDetails>.CreateDefault();

        options.SetBody(request);

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);

        options.QueryParameters.Add(MatchingApiConstants.BirthDate, request.BirthDate);
        options.QueryParameters.Add(MatchingApiConstants.CardNumber, request.CardNumber);
        options.QueryParameters.Add(MatchingApiConstants.NationalCode, request.NationalCode);

        var result =
            await GetAsync<JibitResponseWrapper<JibitMatchResult>>(JibitEndPoints.GetMatchingResult,
                options);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }

    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchIbanWithIdentityDetails(MatchIbanNumberWithIdentityDetails request, string bearerToken)
    {
        var options = RequestOptions<MatchIbanNumberWithIdentityDetails>.CreateDefault();

        options.SetBody(request);

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);

        options.QueryParameters.Add(MatchingApiConstants.BirthDate, request.BirthDate);
        options.QueryParameters.Add(MatchingApiConstants.Iban, request.Iban);
        options.QueryParameters.Add(MatchingApiConstants.NationalCode, request.NationalCode);

        var result =
            await GetAsync<JibitResponseWrapper<JibitMatchResult>>(JibitEndPoints.GetMatchingResult,
                options);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }

    public async Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchAccountNumberWithIdentityDetails(MatchAccountNumberWithIdentityDetails request, string bearerToken)
    {
        var options = RequestOptions<MatchAccountNumberWithIdentityDetails>.CreateDefault();

        options.SetBody(request);

        options.HeaderParameters.Add("Authorization", "Bearer " + bearerToken);

        options.QueryParameters.Add(MatchingApiConstants.BirthDate, request.BirthDate);
        options.QueryParameters.Add(MatchingApiConstants.AccountNumber, request.AccountNumber);
        options.QueryParameters.Add(MatchingApiConstants.NationalCode, request.NationalCode);
        options.QueryParameters.Add(MatchingApiConstants.Bank, request.Bank);

        var result =
            await GetAsync<JibitResponseWrapper<JibitMatchResult>>(JibitEndPoints.GetMatchingResult,
                options);

        result.Result.AddJibitErrorResponse(result);
        if (result.Successed) result.Result.TransferToObject(result.RawContent);
        
        return result;
    }
}