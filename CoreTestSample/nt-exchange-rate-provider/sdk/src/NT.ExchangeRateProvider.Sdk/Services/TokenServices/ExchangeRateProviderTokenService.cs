using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Models.Responses;
using NT.SDK.ExchangeRateProvider.Resources;
using NT.SDK.RestClient.Clients;
using NT.SDK.RestClient.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace NT.SDK.ExchangeRateProvider.Services.TokenServices;

public class ExchangeRateProviderTokenService(
    ApiClient apiClient,
    IRedisDatabase redisDatabase,
    ILogger<ExchangeRateProviderTokenService> logger,
    ExchangeRateProviderOptions options) : BaseApi, IExchangeRateProviderTokenService
{

    public async Task<GetTokenResponse> GetTokenAsync(GetTokenRequest tokenRequest, bool renewToken = false, CancellationToken cancellationToken = default)
    {
        var tokenKey = RedisKeys.GetTokenKey(options.CachePrefix);
        if (!renewToken && await redisDatabase.ExistsAsync(tokenKey))
            return await redisDatabase.GetAsync<GetTokenResponse>(tokenKey);
        else
            return await GetExrpToken(tokenRequest, tokenKey, cancellationToken);
    }

    private async Task<GetTokenResponse> GetExrpToken(GetTokenRequest tokenRequest, string tokenKey, CancellationToken cancellationToken)
    {
        RequestOptions<GetTokenRequest> requestOptions = RequestOptions<GetTokenRequest>.CreateDefault("Exrp.SDK");

        tokenRequest.Validate();
        requestOptions.SetBody(tokenRequest);

        try
        {
            logger.LogInformation("Sending Exrp token request. RequestOptions: {@RequestOptions}", requestOptions);

            ApiResponse<GetTokenResponse> apiResponse =
                await apiClient.PostAsync<GetTokenResponse, GetTokenRequest>(
                    Endpoints.Get_Token, requestOptions, cancellationToken).ConfigureAwait(false);

            if (apiResponse == null || string.IsNullOrWhiteSpace(apiResponse.RawContent))
            {
                logger.LogWarning("Exrp token request returned an empty response. Possible Errors: wrong BasePath, wrong configuration for redis or vault configuration. could not connect to the bank server for getting token. Metadata: {metaData}", new
                {
                    Endpoints.Get_Token,
                });

                throw new ApplicationException("Received empty response from Exrp token API. for more Infomration check Logs.");
            }

            logger.LogDebug("Received response from Exrp API. StatusCode: {StatusCode}, Headers: {@Headers}",
                apiResponse.StatusCode, apiResponse.Headers);

            GetTokenResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<GetTokenResponse>(apiResponse.RawContent);
                options.TokenExpireInSec = response.ExpireInSec;
            }
            catch (JsonException jsonEx)
            {
                logger.LogCritical(jsonEx, "Error deserializing Exrp token response. ResponseContent: {RawContent}, Erorr Message: {ErorrMessage}",
                    apiResponse.RawContent, jsonEx.Message);
                throw new ApplicationException($"Error parsing Exrp token response. message: {jsonEx.Message}");
            }


            try
            {
                await redisDatabase.AddAsync(tokenKey, response, expiresIn: TimeSpan.FromSeconds(options.TokenExpireInSec));
                logger.LogDebug("Exrp token stored in Redis. RedisKey: {RedisKey}, Expiry: {Expiry} seconds",
                    tokenKey, options.TokenExpireInSec);
            }
            catch (Exception redisEx)
            {
                logger.LogCritical(redisEx, "Failed to store Exrp token in Redis. RedisKey: {RedisKey}",
                   tokenKey);
            }

            logger.LogInformation("Exrp token received successfully. Response: {@Response}",
                response);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error retrieving Exrp token. ExceptionMessage: {ExceptionMessage}",
                 ex.Message);
            throw new ApplicationException("Error retrieving Exrp token.");
        }
    }
}
