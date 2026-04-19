using ExchangeRateProvider.Adapter.Base.Services;
using ExchangeRateProvider.Adapter.CryptoCompare.Clients;
using ExchangeRateProvider.Adapter.CryptoCompare.Models.Responses.Apis;
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
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Services
{
    public class ApiRateServices(
        IProviderBusinessLogicQueryRepository providerBusinessLogicQueryRepository,
        EncryptionConfiguration encryptionConfiguration,
        IMarketTradingPairQueryRepository tradingPairQueryRepository,
        IProviderApiAccountQueryRepository providerApiAccountQueryRepository,
        CryptoCompareApiClient apiClient,
        IRedisService redisService,
        ILogger<ApiRateServices> logger) : AdapterBaseService(providerBusinessLogicQueryRepository,
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
                (int providerId, ProviderApiAccountCredentials credentials) = await GetAccountApiKey(ProviderType.CryptoCompare, providerApiAccountId, cancellationToken);
                IEnumerable<MarketTradingPair> pairs = await GetPairs(CurrencyType.Crypto, providerId, cancellationToken);
                var formattedPairs = FormatTradingPairs(pairs);

                RequestOptions requestOptions = RequestOptions.CreateDefault("CryptoCompare-Api");

                var uri = UrlGenerator.GenerateCryptoCompareApiUrl(credentials.ApiKey, "binance", formattedPairs);
                ApiResponse<LatestTickResponse> apiResponse = await apiClient.GetAsync<LatestTickResponse>(uri, requestOptions, cancellationToken).ConfigureAwait(false);

                var result = GenerateResult(apiResponse, nameof(StreamRates));

                foreach (var data in result.Data)
                    await redisService.SavePairsPriceDataToRedisAsync(ProviderType.CryptoCompare, data.Key, data.Value.Price, data.Value.CurrentDayVolume, data.Value.Market);

                logger.LogInformation("Data Recived Successfully.");
            }
            catch (Exception ex)
            {
                logger.LogCritical("an error occured while calling cryptoCompare Apis. ex: {ex}", ex.Message);
                throw;
            }
        }
    }
}
