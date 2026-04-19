using ExchangeRateProvider.Adapter.CryptoCompare.Models.Options;
using ExchangeRateProvider.Adapter.CryptoCompare.Services;
using Microsoft.Extensions.Logging;
using NT.SDK.RestClient.Clients;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Clients
{
    public class CryptoCompareApiClient(CryptoCompareConfigurations configurations, ILogger<ApiRateServices> logger) : ApiClient(configurations, logger)
    {
    }
}
