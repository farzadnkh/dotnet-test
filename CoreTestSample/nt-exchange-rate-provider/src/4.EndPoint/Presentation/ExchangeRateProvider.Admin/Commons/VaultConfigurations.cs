using ExchangeRateProvider.Adapter.CryptoCompare;
using ExchangeRateProvider.Adapter.Xe;
using ExchangeRateProvider.Admin.Commons;
using ExchangeRateProvider.Admin.Workers;
using ExchangeRateProvider.Application.Commons;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Infrastructure.Sql.Commons;
using Hangfire;
using Hangfire.Redis.StackExchange;
using NT.HashiCorp.Vault.Abstraction;
using NT.Logs.ElasticApm.HashicorpVault.Extensions;
using NT.Logs.Logging.HashicorpVault.Extensions;
using NT.MassTransit.HashicorpVault.Extensions;
using StackExchange.Redis;
using System.Threading.Channels;

namespace ExchangeRateProvider.Admin.Commons;

public static class VaultConfigurations
{
    internal static async Task<WebApplicationBuilder> ConfigureVaultServerAsync(this WebApplicationBuilder builder)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        await builder.AddHashiCorpVaultAsync(option =>
        {
            option.EnvironmentFileName = ".env";
            option.JsonFileName = "vault.credentials.json";
            option.VaultServiceTimeout = TimeSpan.FromSeconds(60);
        },
        HachiCorpVaultImplementationsAsync, isDebugMode: environment.Equals("Debug", StringComparison.CurrentCultureIgnoreCase),
            cancellationToken: default);

        return builder;
    }

    private static async Task HachiCorpVaultImplementationsAsync(IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.HealthCheckAsync(cancellationToken);

        var baseUri = await context.GetBaseUrisAsync(cancellationToken);
        context.AddAuthentication(baseUri.IdpBaseUri);

        // await context.AddObservabilityAsync(cancellationToken);
        await context.AddInfrastructuresAsync(cancellationToken);
        await context.ConfigureMassTransitAsync(cancellationToken);
        context.AddApplication(cancellationToken);
        context.Services.AddXeRestClient(baseUri.XeCdApiBaseUri);
        context.Services.AddCryptoCompareRestClient(baseUri.CryptoCompareProviderApiBaseUri);

        BoundedChannelOptions option = new(100)
        {
            SingleReader = false,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite,
        };

        ConfigurationOptions redisConfig = await context.GetRedisConnectionOption();
        await context.AddRedis();
        await context.AddHangFireAsync(redisConfig, cancellationToken);

        context.Services.AddActivatedSingleton((_) => Channel.CreateBounded<BackgroundJobSyncMessageArgs>(option));
        context.Services.AddActivatedSingleton((_) => Channel.CreateBounded<JobsIntervalMessageArgs>(option));
        context.Services.AddHostedService<JobManagement>();
    }

    private static async Task AddObservabilityAsync(this IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.AddLogsAsync(optionAction: options =>
        {
            context.Configurations.GetSection("LoggerOptions").Bind(options);
        },
        pathAction: path =>
        {
            context.Configurations.GetSection("VaultOption:LogsSecrets").Bind(path);
        }, cancellationToken: cancellationToken);

        await context.AddElasticApmServerAsync(optionAction: options =>
        {
            options.HttpDiagnostic.Enable = true;
            options.EfCoreDiagnostic.Enable = true;
            options.ElasticsearchDiagnostic.Enable = true;
            options.SqlClientDiagnostic.Enable = true;
        },
            pathAction: path =>
            {
                context.Configurations.GetSection("VaultOption:ElasticSecrets").Bind(path);
            }
        );
    }

    private static void AddAuthentication(this IHashiCorpVaultContext context, string idpbaseUrl)
    {
        context.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies")
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = idpbaseUrl;
            options.RequireHttpsMetadata = false;

            options.ClientId = "cpl-client";
            options.ClientSecret = "D4VHwlCHEp1oUEkNyBAE+SCw5BtFUu/Ghbgv05zn2bDiRIZDtlrMsifmpVDuUEH+xQe0xqX2yFJbwBmn7eq9hg==";
            options.ResponseType = "code id_token";
            options.SaveTokens = true;

            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("offline_access");
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = idpbaseUrl;
            options.Audience = "realtime-api";
            options.RequireHttpsMetadata = false;
        });
    }

    private static async Task AddHangFireAsync(this IHashiCorpVaultContext context, ConfigurationOptions redisConfig, CancellationToken cancellationToken)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(redisConfig);
        var redisOptions = context.Configurations.GetSection("RedisOption").Get<RedisOption>() ?? new();

        context.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(redis, new RedisStorageOptions
            {
                Db = redisOptions.HangFireDatabase,
            }));

        context.Services.AddHangfireServer(options =>
        {
            options.Queues = ["default", "api_call", "socket_call"];
        });
    }

    private async static Task<ConfigurationOptions> GetRedisConnectionOption(this IHashiCorpVaultContext context)
    {
        var globalMountPoint = context.Configurations["VaultOption:GlobalCredentialsMountPoint"];
        await context.GetAndSetConfigurationAsync<RedisConfigurations>("RedisCredentials", globalMountPoint);

        var redisConfigurations = context.Configurations.GetSection("RedisCredentials").Get<RedisConfigurations>() ?? new();
        var redisConnectionOption = new ConfigurationOptions
        {
            EndPoints = { $"{redisConfigurations.Host}:{redisConfigurations.Port}" },
            User = redisConfigurations.UserName,
            Password = redisConfigurations.Password,
            Ssl = false,
            AbortOnConnectFail = false
        };
        return redisConnectionOption;
    }

    private static async Task<IHashiCorpVaultContext> ConfigureMassTransitAsync(
    this IHashiCorpVaultContext context,
    CancellationToken cancellationToken)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Debug";
        await context.AddMassTransitAsync(rmo =>
        {
            rmo.ServiceName = $"ExchangeRateProvider_{env}_cpl";
            rmo.ServiceReleaseEnvironment = env;
        }, busRegConf => { },
        (busRegContext, rabbitBusFactoryConf) =>
        {
            rabbitBusFactoryConf.Message<PriceChangedEventMessageArgs>(x => x.SetEntityName($"{env}_PriceChangeQueue"));
            rabbitBusFactoryConf.Message<NewPriceChangeStreamedMessageArgs>(x => x.SetEntityName($"{env}_NewPriceChangeStreamQueue"));
            rabbitBusFactoryConf.Message<SocketSyncMessageArgs>(x => x.SetEntityName($"{env}_SocketSyncQueue"));
            rabbitBusFactoryConf.Message<SettingModel>(x => x.SetEntityName($"{env}_SettingSyncQueue"));
        }, pathAction =>
        {
            context.Configurations.GetSection("VaultOption:MasstransitSecrets").Bind(pathAction);
        });

        return context;
    }
}

