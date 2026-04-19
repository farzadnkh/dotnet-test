using ExchangeRateProvider.Application.Consumers.Extensions;
using ExchangeRateProvider.Application.Settings.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Dtos;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Contract.Settings.Services;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;
using MessagePack;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace ExchangeRateProvider.Application.Consumers.Services;

public class PairAggregator(
    IConsumerQueryRepository consumerQueryRepository,
    IRedisDatabase redisDatabase,
    IExchangeRateSnapshotMemory exchangeRateSnapshotMemory,
    ILogger<PairAggregator> logger,
    ISettingService settingService,
    ISettingQueryRepository settingQueryRepository)
    : IPairAggregator
{
    
    public async Task<PairExchangeRateDto> GetPairExchangeRate(int consumerId, TradingPairSnapshot tradingPair)
    {
        try
        {
            logger.LogInformation("Fetching exchange rate for ConsumerId: {ConsumerId}, Pair: {Pair}", consumerId, tradingPair.Id);

            SettingModel settings = await settingService.LoadSettingsAsync(settingQueryRepository);
            logger.LogDebug("Settings loaded for ConsumerId: {ConsumerId}", consumerId);

            var consumer = await GetOrAddConsumerAsync(consumerId);
            var spreadOptions = GetOrAddSpreadOptions(consumer, tradingPair);

            var redisLookup = await GetRedisRatesLookupAsync(tradingPair);
            var listOfPrices = CalculateProviderPrices(tradingPair, consumer, settings, redisLookup);

            var finalPrice = CalculateFinalPrice(tradingPair.CalculationTerm, listOfPrices);
            var (upperLimit, lowerLimit) = spreadOptions.GetSpreadPrice(finalPrice);

            var roundedFinalPrice = Math.Round(finalPrice, tradingPair.DecimalPrecision ?? 4, MidpointRounding.ToZero);
            var roundedUpperLimit = Math.Round(upperLimit, tradingPair.DecimalPrecision ?? 4, MidpointRounding.ToZero);
            var roundedLowerLimit = Math.Round(lowerLimit, tradingPair.DecimalPrecision ?? 4, MidpointRounding.ToZero);

            var lastPrice = exchangeRateSnapshotMemory.TryGetLastExposedPrice(consumerId, tradingPair.Id);
            if (lastPrice is not null && roundedFinalPrice == lastPrice.Value)
            {
                logger.LogDebug("Price hasn't changed for ConsumerId: {ConsumerId}, Pair: {Pair}", consumerId, tradingPair.Id);
                return null;
            }

            exchangeRateSnapshotMemory.SetLastExposedPrice(consumerId, tradingPair.Id, roundedFinalPrice);
            logger.LogInformation("Final exchange rate calculated: {Price}", roundedFinalPrice);

            return new PairExchangeRateDto($"{tradingPair.CurrencyCode}{tradingPair.BaseCurrencyCode}", roundedFinalPrice, roundedUpperLimit, roundedLowerLimit, spreadOptions.UpperLimitPercentage, spreadOptions.LowerLimitPercentage);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error occurred while calculating exchange rate for ConsumerId: {ConsumerId}, Pair: {Pair}", consumerId, tradingPair.Id);
            return null;
        }
    }

    private async Task<Consumer> GetOrAddConsumerAsync(int consumerId)
    {
        var cached = exchangeRateSnapshotMemory.TryGetConsumer(consumerId);
        if (cached is not null)
            return cached;

        var consumer = await consumerQueryRepository.GetReadOnlyConfigurationByConsumerIdWithAllIncludesAsync(consumerId);
        exchangeRateSnapshotMemory.SetConsumer(consumerId, consumer);
        return consumer;
    }

    private SpreadOptions GetOrAddSpreadOptions(Consumer consumer, TradingPairSnapshot tradingPair)
    {
        var (result, cachedOptions) = exchangeRateSnapshotMemory.TryToGetSpreadOptionsMemory(consumer.Id, tradingPair.Id);
        if (result)
            return cachedOptions;

        var options = CreateSpreadOptions(consumer, tradingPair);
        exchangeRateSnapshotMemory.UpdateSpreadOptionsMemory(consumer.Id, tradingPair.Id, options);
        return options;
    }

    private SpreadOptions CreateSpreadOptions(Consumer consumer, TradingPairSnapshot tradingPair)
    {
        var consumerPair = consumer.ConsumerPairs.FirstOrDefault(cp =>
            cp.PairId == tradingPair.Id && cp.IsActive);

        var consumerMarket = consumer.ConsumerMarkets.FirstOrDefault(cm =>
            cm.MarketId == tradingPair.MarketId && cm.IsActive);

        return new SpreadOptionStruct(consumerPair, consumerMarket, tradingPair, consumerMarket?.Market)
            .GetActiveSpreadOption();
    }

    private async Task<Dictionary<string, byte[]>> GetRedisRatesLookupAsync(TradingPairSnapshot tradingPair)
    {
        try
        {
            var key = RedisKeys.GenerateTradingPairKey(tradingPair.BaseCurrencyCode, tradingPair.CurrencyCode);
            var entries = await redisDatabase.HashGetAllAsync<byte[]>(key);
            return entries.ToDictionary(e => e.Key, e => e.Value);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "There is an error occurred during Reading Prices from Redis");
            throw ApplicationBadRequestException.Create("Something Went Wrong During Reading Prices");
        }
    }

    private List<decimal> CalculateProviderPrices(
        TradingPairSnapshot tradingPair,
        Consumer consumer,
        SettingModel settings,
        Dictionary<string, byte[]> redisLookup)
    {
        var result = new List<decimal>();
        if (tradingPair.ProviderIds.Count <= 0) return result;

        foreach (var providerLink in tradingPair.ProviderIds)
        {
            var consumerProvider = consumer.ConsumerProviders
                .FirstOrDefault(cp =>
                    cp.Provider.Id == providerLink &&
                    cp.IsActive &&
                    cp.Provider.Published);

            if (consumerProvider == null) continue;
            var providerKey = consumerProvider.Provider.Type.ToString();

            var matchedRates = redisLookup
                .Where(kvp => kvp.Key.Contains(providerKey))
                .Select(kvp => MessagePackSerializer.Deserialize<ExchangeRateValue>(kvp.Value))
                .ToList();

            if (matchedRates.Count <= 0) continue;
            var filteredRates = ExchangeRateFilterHelper.FilterOutliers(matchedRates, settings.TimeDifference, settings.PercentageDifference);
            if (filteredRates.Count <= 0) continue;
            result.AddRange(filteredRates.Select(i => i.Price));
        }

        return result;
    }

    private static decimal CalculateFinalPrice(MarketCalculationTerm term, List<decimal> prices) =>
        term switch
        {
            MarketCalculationTerm.Average => prices.DefaultIfEmpty().Average(),
            MarketCalculationTerm.MostProfit => prices.DefaultIfEmpty().Max(),
            MarketCalculationTerm.LowestProfit => prices.DefaultIfEmpty().Min(),
            _ => 0
        };
}
