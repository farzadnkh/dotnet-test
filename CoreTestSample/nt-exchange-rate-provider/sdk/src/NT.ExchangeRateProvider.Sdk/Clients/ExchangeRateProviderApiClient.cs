using Microsoft.Extensions.Logging;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Services.RateServices;
using NT.SDK.ExchangeRateProvider.Services.TokenServices;
using NT.SDK.RestClient.Clients;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace NT.SDK.ExchangeRateProvider.Clients;

public class ExchangeRateProviderApiClient : ApiClient, IExchangeRateProviderApiClient
{
    public ExchangeRateProviderApiClient(
        ExchangeRateProviderOptions options,
        IRedisDatabase redisDatabase,
        ILogger<ExchangeRateProviderTokenService> tokenLogger,
        ILogger<ExchangeRateProviderRateService> rateServiceLogger) : base(options.Configuration)
    {
        ExchangeRateProviderTokenService = AddApi(new ExchangeRateProviderTokenService(this, redisDatabase, tokenLogger, options));
        ExchangeRateProviderRateService = AddApi(new ExchangeRateProviderRateService(this, rateServiceLogger, ExchangeRateProviderTokenService, options));
    }

    public IExchangeRateProviderTokenService ExchangeRateProviderTokenService { get; }

    public IExchangeRateProviderRateService ExchangeRateProviderRateService { get; }

    public string GetBasePath()
    {
        return Configuration.BasePath;
    }
}


