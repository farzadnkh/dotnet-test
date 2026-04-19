using NT.SDK.ExchangeRateProvider.Services.RateServices;
using NT.SDK.ExchangeRateProvider.Services.TokenServices;
using NT.SDK.RestClient.Clients;

namespace NT.SDK.ExchangeRateProvider.Clients;

public interface IExchangeRateProviderApiClient : IApiAccessor
{
    IExchangeRateProviderTokenService ExchangeRateProviderTokenService { get; }
    IExchangeRateProviderRateService ExchangeRateProviderRateService { get; }
}
