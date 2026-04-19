using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Models.Responses;
using NT.SDK.ExchangeRateProvider.Resources;
using NT.SDK.ExchangeRateProvider.Services.TokenServices;
using NT.SDK.RestClient.Clients;
using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.SDK.ExchangeRateProvider.Services.RateServices;

public class ExchangeRateProviderRateService(
    ApiClient apiClient,
    ILogger<ExchangeRateProviderRateService> logger,
    IExchangeRateProviderTokenService tokenService,
    ExchangeRateProviderOptions options) : BaseApi, IExchangeRateProviderRateService
{
    public async Task<ExrpApiResponseWrapper<GetLatestPriceResponse>> GetLatestPriceAsync(
        GetLatestPriceRequest request,
        GetTokenRequest tokenRequest,
        CancellationToken cancellationToken)
    {
        var result = new ExrpApiResponseWrapper<GetLatestPriceResponse>();

        try
        {
            var token = await tokenService.GetTokenAsync(tokenRequest, false, cancellationToken);
            if (token == null || string.IsNullOrEmpty(token.Token))
            {
                result.Code = HttpStatusCode.Unauthorized;
                result.Success = false;
                result.Message = "Failed to retrieve token.";
                result.AddError("401", "Authorization token is null.");
                logger.LogError("Exrp: Token retrieval failed for request {@Request}", tokenRequest);
                return result;
            }

            var requestOptions = RequestOptions<GetLatestPriceRequest>.CreateDefault("Exrp.SDK");
            _ = requestOptions.SetHeaderParameters(new Multimap<string, string> { { "Authorization", $"Bearer {token.Token}" } });
            _ = requestOptions.SetQueryParameters(QueryParameters(request));

            var response = await apiClient.PostAsync<ExrpApiResponseWrapper<GetLatestPriceResponse>, GetLatestPriceRequest>(
                Endpoints.Get_Rates_Api,
                requestOptions,
                cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                result.Code = HttpStatusCode.InternalServerError;
                result.Success = false;
                result.Message = "Exrp: Unexpected null response from API.";
                result.AddError("198", "Null response received from Exrp API.");
                logger.LogError("Exrp: Received null API response for request {@Request}", request);
                return result;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var exrpResponse = JsonConvert.DeserializeObject<GetLatestPriceResponse>(response.RawContent);
                    result.Code = HttpStatusCode.OK;
                    result.SetData(exrpResponse);
                    result.Success = true;
                }
                catch (JsonException jsonEx)
                {
                    result.Code = HttpStatusCode.BadRequest;
                    result.Success = false;
                    result.Message = "Exrp: Failed to deserialize API response.";
                    result.AddError("198", $"Deserialization error: {jsonEx.Message}");
                    logger.LogError(jsonEx, "Exrp: Error deserializing API response: {RawContent}", response.RawContent);
                }
            }
            else
            {
                result.Code = HttpStatusCode.BadRequest;
                result.Success = false;
                result.Message = "Exrp: Error while calling GetRates API.";

                if (response.Result?.Errors != null)
                {
                    foreach (var error in response.Result.Errors)
                    {
                        result.AddError(error.Message, error.Code);
                    }
                }

                logger.LogWarning("Exrp: API returned error. Status: {StatusCode}, Errors: {@Errors}", response.StatusCode, response.Result?.Errors);
            }
        }
        catch (Exception ex)
        {
            result.Code = HttpStatusCode.InternalServerError;
            result.Success = false;
            result.Message = "Exrp: Unexpected error occurred while processing request.";
            result.AddError("500", ex.Message);
            logger.LogError(ex, "Exrp: Unexpected exception occurred for request {@Request}", request);
        }

        return result;
    }

    #region Utilities

    private static Multimap<string, string> QueryParameters(GetLatestPriceRequest request)
    {
        var result = new Multimap<string, string>();
        if (!string.IsNullOrEmpty(request.Market))
            result.Add("Market", request.Market);

        if (!string.IsNullOrEmpty(request.Pairs))
            result.Add("Pairs", request.Pairs);

        if (request.EnableCache.HasValue && request.EnableCache.Value == true)
            result.Add("EnableCache", "true");

        if (request.CacheTtlInSec.HasValue && request.CacheTtlInSec.Value > 0)
            result.Add("CacheTtlInSec", "0");

        if (request.ProviderTypes.HasValue && request.ProviderTypes.Value != Models.Enums.ProviderType.None)
            result.Add("ProviderTypes", request.ProviderTypes.ToString());

        return result;
    }

    #endregion
}
