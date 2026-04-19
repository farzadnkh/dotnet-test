using ExchangeRateProvider.Application.Commons.Channels;
using ExchangeRateProvider.Application.Commons.Services;
using ExchangeRateProvider.Application.Consumers.Services;
using ExchangeRateProvider.Application.Settings.Services;
using ExchangeRateProvider.Application.Sockets.Clients;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Services;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.Settings.Services;
using Microsoft.Extensions.DependencyInjection;
using NT.HashiCorp.Vault.Abstraction;

namespace ExchangeRateProvider.Application.Commons;


public static class InjectionBootstrapper
{
    public static IHashiCorpVaultContext AddApplication(
        this IHashiCorpVaultContext context,
        CancellationToken cancellationToken = default)
    {
        context.Services.AddSingleton<IExchangeRateSnapshotMemory, ExchangeRateSnapshotMemory>();
        context.Services.AddSingleton<ISettingService, SettingService>();
        context.Services.AddSingleton<IChannelRegistry, ChannelRegistry>();
        context.Services.AddSingleton<IRedisService, RedisService>();
        
        context.Services.AddScoped<WebSocketHandler>();
        context.Services.AddScoped<IAggregatorServiceFactory, AggregatorServiceFactory>();
        context.Services.AddScoped<IApiKeyClientService, ApiKeyClientService>();
        context.Services.AddScoped<IConsumerPairAggregator, ConsumerAggregator>();
        context.Services.AddScoped<IPairAggregator, PairAggregator>();

        return context;
    }
}

