using ExchangeRateProvider.Adapter.Xe.Models.Options;
using ExchangeRateProvider.Adapter.Xe.Services;
using Microsoft.Extensions.Logging;
using NT.SDK.RestClient.Clients;

namespace ExchangeRateProvider.Adapter.Xe.Clients
{
    public class XeApiClient(XeConfigurations configurations, ILogger<XeApiRateServices> logger) : ApiClient(configurations, logger)
    {
    }
}
