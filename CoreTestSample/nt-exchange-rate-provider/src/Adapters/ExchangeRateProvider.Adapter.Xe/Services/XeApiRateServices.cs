using ExchangeRateProvider.Adapter.Base.Services;
using ExchangeRateProvider.Adapter.Xe.Clients;
using ExchangeRateProvider.Adapter.Xe.Models.Requests;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Commons.Services;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using Hangfire;
using Microsoft.Extensions.Logging;
using NT.SDK.RestClient.Models;
using System.Text;

namespace ExchangeRateProvider.Adapter.Xe.Services;

public class XeApiRateServices(
        IProviderBusinessLogicQueryRepository providerBusinessLogicQueryRepository,
        EncryptionConfiguration encryptionConfiguration,
        IMarketTradingPairQueryRepository tradingPairQueryRepository,
        IProviderApiAccountQueryRepository providerApiAccountQueryRepository,
        ILogger<XeApiRateServices> logger,
        IRedisService redisService,
        XeApiClient apiClient) : AdapterBaseService(providerBusinessLogicQueryRepository,
                                                    providerApiAccountQueryRepository,
                                                    encryptionConfiguration,
                                                    tradingPairQueryRepository,
                                                    logger)
{
    [Queue("api_call")]
    [DisableConcurrentExecution(timeoutInSeconds: 10)]
    public async Task StreamRates(int providerApiAccountId, CancellationToken cancellationToken)
    {
        try
        {
            (int providerId, ProviderApiAccountCredentials credentials) = await GetAccountApiKey(ProviderType.XE, providerApiAccountId, cancellationToken);
            IEnumerable<MarketTradingPair> pairs = await GetPairs(CurrencyType.Fiat, providerId, cancellationToken);
            Dictionary<Market, List<MarketTradingPair>> formattedPairs = FormatTradingMarketPairs(pairs);

            RequestOptions requestOptions = RequestOptions.CreateDefault("Xe-Api");
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ApiAccount}:{credentials.Token}"));
            requestOptions.HeaderParameters.Add("Authorization", $"Basic {authToken}");
            requestOptions.HeaderParameters.Add("Accept", "application/json");

            foreach (var item in formattedPairs)
            {
                foreach (var instrument in item.Value.Select(item => item.Currency.Code))
                {
                    var uri = UrlGenerator.GenerateXeApiUrl(item.Key.BaseCurrency.Code, instrument);
                    ApiResponse<GetRateResponse> apiResponse = await apiClient.GetAsync<GetRateResponse>(uri, requestOptions, cancellationToken).ConfigureAwait(false);

                    var result = GenerateResult(apiResponse, nameof(StreamRates));

                    foreach (var data in result.To)
                        await redisService.SavePairsPriceDataToRedisAsync(ProviderType.XE, $"{instrument}{data.Quotecurrency}", data.Mid, 0, null);

                    logger.LogInformation("Data Received Successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical("an error occurred while calling Xe Apis. ex: {ex}", ex.Message);
            throw;
        }
    }
}
