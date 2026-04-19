using ExchangeRateProvider.Application.ExchangeRateProviders.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NT.DDD.Application.Exceptions;
using NT.SDK.RestClient.Models;
using StackExchange.Redis;
using System.Net;

namespace ExchangeRateProvider.Adapter.Base.Services;

public abstract class AdapterBaseService(
    IProviderBusinessLogicQueryRepository providerBusinessLogicQueryRepository,
    IProviderApiAccountQueryRepository providerApiAccountQueryRepository,
    EncryptionConfiguration encryptionConfiguration,
    IMarketTradingPairQueryRepository tradingPairQueryRepository,
    ILogger logger)
{
    public virtual async Task<(int ProviderId, ProviderApiAccountCredentials credentials)> GetAccountApiKey(ProviderType provider, int providerApiAccountId, CancellationToken token)
    {
        ExchangeRateProviderApiAccount providerApiAccount = new();
        if (providerApiAccountId <= 0)
            providerApiAccount = await providerApiAccountQueryRepository.GetByProviderTypePublishedWithAllIncludesAsync(provider, token);
        else
            providerApiAccount = await providerApiAccountQueryRepository.GetByIdWithAllIncludesAsync(providerApiAccountId, token);


        var credentials = providerApiAccount.Credentials.GetCredentials(encryptionConfiguration.EncryptionKey);
        if (credentials == null)
        {
            logger.LogCritical("API Key is missing or invalid.");
            throw new ApplicationException($"Invalid {provider} API key.");
        }

        return (providerApiAccount.ProviderId, credentials);
    }

    public virtual async Task<IEnumerable<MarketTradingPair>> GetPairs(CurrencyType currencyType, int providerId, CancellationToken cancellationToken)
    {
        var pairs = await tradingPairQueryRepository
            .GetAllPublishedByProviderIdAndCurrencyTypeWithAllIncludesAsync(providerId, currencyType, cancellationToken);

        if (!pairs.Any())
        {
            logger.LogWarning("No pairs registered for provider {providerId}.", providerId);
            throw ApplicationBadRequestException.Create($"No pairs registered for provider: {providerId}");
        }

        return pairs;
    }

    public virtual List<string> FormatTradingPairs(IEnumerable<MarketTradingPair> tradingPairs)
    {
        ArgumentNullException.ThrowIfNull(tradingPairs);
        return [.. tradingPairs.Select(pair =>
        {
            var currency = pair.Currency?.Code ?? "UnknownCurrency";
            var baseCurrency = pair.Market?.BaseCurrency?.Code ?? "UnknownBase";
            return $"{currency}-{baseCurrency}";
        })];
    }

    public virtual Dictionary<Market, List<MarketTradingPair>> FormatTradingMarketPairs(IEnumerable<MarketTradingPair> tradingPairs)
    {
        ArgumentNullException.ThrowIfNull(tradingPairs);
        return tradingPairs
           .GroupBy(item => item.Market)
           .ToDictionary(
               g => g.Key,
               g => g.ToList()
           );
    }

    public virtual T GenerateResult<T>(ApiResponse<T> apiResponse, string methodName) where T : class, new()
    {
        if (!apiResponse.Successed || apiResponse.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError($"Failed to call {methodName}, Reqeust error is: {apiResponse.RawContent}");
            throw ApplicationBadRequestException.Create("Failed To call Api. For More Information Check Logs.");
        }
        else
        {
            if (apiResponse.Result is not null)
                return apiResponse.Result;

            try
            {
                var result = JsonConvert.DeserializeObject<T>(apiResponse.RawContent);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogCritical("Can not Deserialize Api Response From Method: {methodName}, the Raw Content is: {Raw}", methodName, apiResponse.RawContent);
                throw ApplicationBadRequestException.Create("an error occurred while processing the Api response.");
            }
        }
    }

    public virtual async Task<IEnumerable<ProviderBusinessLogic>> GetAllBusinessLogicsAsync(int providerId)
        => await providerBusinessLogicQueryRepository.GetAllByProviderIdAsync(providerId, default);
}
