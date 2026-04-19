using ExchangeRateProvider.Adapter.Xe.Clients;
using ExchangeRateProvider.Adapter.Xe.Models.Options;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRateProvider.Adapter.Xe
{
    public static class XeClientBootstrapper
    {
        public static IServiceCollection AddXeRestClient(this IServiceCollection services, string xeBaseUri)
        {
            XeConfigurations configuration = new()
            {
                Timeout = 60_000,
                BasePath = xeBaseUri
            };

            services.AddSingleton(configuration);
            services.AddSingleton<XeApiClient>();

            return services;
        }
    }
}
