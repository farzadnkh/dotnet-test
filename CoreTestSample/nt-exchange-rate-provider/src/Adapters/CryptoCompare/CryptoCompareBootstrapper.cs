using ExchangeRateProvider.Adapter.CryptoCompare.Clients;
using ExchangeRateProvider.Adapter.CryptoCompare.Models.Options;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRateProvider.Adapter.CryptoCompare
{
    public static class CryptoCompareBootstrapper
    {
        public static IServiceCollection AddCryptoCompareRestClient(this IServiceCollection services, string cryptoCompareBaseUri)
        {
            CryptoCompareConfigurations configuration = new()
            {
                Timeout = 60_000,
                BasePath = cryptoCompareBaseUri
            };
            services.AddSingleton(configuration);
            services.AddSingleton<CryptoCompareApiClient>();

            return services;
        }
    }
}
