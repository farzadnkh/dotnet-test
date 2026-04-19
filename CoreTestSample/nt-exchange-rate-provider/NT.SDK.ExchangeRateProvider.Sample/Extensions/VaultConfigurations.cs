using Hangfire;
using Hangfire.Redis.StackExchange;
using NT.Caching.Redis.HashiCorpVault.Extensions;
using NT.HashiCorp.Vault.Abstraction;
using NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Extensions;
using NT.SDK.ExchangeRateProvider.Sample.Workes;
using StackExchange.Redis;

namespace NT.SDK.ExchangeRateProvider.Sample.Extensions;

public static class VaultConfigurations
{
    internal static async Task<WebApplicationBuilder> ConfigureVaultServerAsync(this WebApplicationBuilder builder)
    {
        await builder.AddHashiCorpVaultAsync(option =>
        {
            option.EnvironmentFileName = ".env";
            option.JsonFileName = "vault.credentials.json";
            option.VaultServiceTimeout = TimeSpan.FromSeconds(60);
        },
        HachiCorpVaultImplementationsAsync, isDebugMode: true,
            cancellationToken: default);

        return builder;
    }

    private static async Task HachiCorpVaultImplementationsAsync(IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.HealthCheckAsync(cancellationToken: cancellationToken);
        await context.AddRedis();
        await context.AddExRP();
        await context.AddHangFireAsync(cancellationToken);
        context.Services.AddHostedService<WebSocketStreamManager>();
    }

    private static async Task AddRedis(this IHashiCorpVaultContext context)
    {
        await context.AddStackExchangeRedisAsync(optionAction: opt =>
        {
            opt.Database = 4; // HARD CODED FOR SAMPLE 

        }, pathAction: path =>
        {
            context.Configurations.GetSection("VaultOption:RedisSecrets").Bind(path);
        });
    }

    private static async Task AddExRP(this IHashiCorpVaultContext context)
    {
        await context.AddExchangeRateProvider(pathAction: path =>
        {
            context.Configurations.GetSection("VaultOption:ExchangeRateSecrets").Bind(path);
        }, option: o =>
        {
            context.Configurations.GetSection("ExchangeRateProivder").Bind(o);
        });
    }

    private static async Task AddHangFireAsync(this IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.GetAndSetConfigurationAsync<RedisConfigurations>("RedisCredentials", "exrp_credentials");

        var redisConfigurations = context.Configurations.GetSection("RedisCredentials").Get<RedisConfigurations>() ?? new();
        var redisConnectionOption = new ConfigurationOptions
        {
            EndPoints = { $"{redisConfigurations.Host}:{redisConfigurations.Port}" },
            User = redisConfigurations.UserName,
            Password = redisConfigurations.Password,
            Ssl = false,
            AbortOnConnectFail = false
        };

        var redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionOption);
        context.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(redis, new RedisStorageOptions
            {
                Db = 4 // HARD CODED FOR SAMPLE 
            }));

        context.Services.AddHangfireServer(options =>
        {
            options.Queues = ["default"];
        });
    }
}
