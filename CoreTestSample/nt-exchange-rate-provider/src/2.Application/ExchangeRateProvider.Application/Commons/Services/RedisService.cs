using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Dtos;
using ExchangeRateProvider.Contract.Commons.Services;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using MessagePack;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Collections.Concurrent;

namespace ExchangeRateProvider.Application.Commons.Services;

internal class RedisService(
    IRedisDatabase redisDatabase,
    INotifier notifier,
    ILogger<RedisService> logger) : IRedisService
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, decimal>> _providerLastPrices = new();

    public async Task SavePairsPriceDataToRedisAsync(ProviderType provider, string pair, decimal price, decimal volume, string market = "")
    {
        try
        {
            var key = RedisKeys.GeneratePairKeys(provider, market);
            var normalizedPair = pair.Replace("-", "");
            var pairPrices = _providerLastPrices.GetOrAdd(key, _ => new ConcurrentDictionary<string, decimal>());
            bool shouldUpdate = false;

            pairPrices.AddOrUpdate(normalizedPair,
                addValueFactory: _ =>
                {
                    shouldUpdate = true;
                    return price;
                },
                updateValueFactory: (_, oldPrice) =>
                {
                    if (oldPrice != price)
                    {
                        shouldUpdate = true;
                        return price;
                    }

                    return oldPrice;
                });


            if (shouldUpdate)
            {
                var hashKey = RedisKeys.GeneratePairHashKey(normalizedPair);

                var data = new ExchangeRateValue
                {
                    Price = price,
                    Volume = volume,
                    Ticks = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                var packed = MessagePackSerializer.Serialize(data);

                await redisDatabase.HashSetAsync(hashKey, key, packed);

                await notifier.NotifyPriceChangedAsync(new PriceChangedEventMessageArgs
                {
                    Provider = provider,
                    Pair = pair,
                    Price = price,
                    Volume = volume,
                    Market = market,
                    Ticks = data.Ticks
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save MessagePack value to Redis for pair {Pair}", pair);
        }
    }
}
