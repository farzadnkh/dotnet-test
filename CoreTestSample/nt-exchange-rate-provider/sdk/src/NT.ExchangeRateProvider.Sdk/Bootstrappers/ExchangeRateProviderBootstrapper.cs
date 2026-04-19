using Microsoft.Extensions.DependencyInjection;
using NT.SDK.ExchangeRateProvider.Clients;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Services.StreamRateServices;
using NT.SDK.RestClient.Models;

namespace NT.SDK.ExchangeRateProvider.Bootstrappers;

public static class ExchangeRateProviderBootstrapper
{
    public static IServiceCollection AddExchangeRateProvider(
        this IServiceCollection services,
        Action<ExchangeRateProviderOptions> options = null)
    {
        ExchangeRateProviderOptions exrpOptions = new();
        options?.Invoke(exrpOptions);
 
        ArgumentException.ThrowIfNullOrWhiteSpace(exrpOptions.BasePath);

        var wrapper = new ExchangeRateProviderOptions
        {
            Configuration = new Configuration()
            {
                Timeout = 60_000,
                BasePath = exrpOptions.BasePath
            },
            BasePath = exrpOptions.BasePath,
            CachePrefix = exrpOptions.CachePrefix,
            TokenExpireInSec = exrpOptions.TokenExpireInSec
        };

        services.AddSingleton(wrapper);
        services.AddSingleton<IExchangeRateProviderApiClient, ExchangeRateProviderApiClient>();
        services.AddSingleton<IExchangeRateProviderStreamRateService, ExchangeRateProviderStreamRateService>();

        return services;
    }
}
