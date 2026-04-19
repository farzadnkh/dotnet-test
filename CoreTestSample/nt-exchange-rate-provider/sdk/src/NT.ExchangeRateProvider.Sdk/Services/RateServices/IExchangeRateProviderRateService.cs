using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Models.Responses;

namespace NT.SDK.ExchangeRateProvider.Services.RateServices;

public interface IExchangeRateProviderRateService
{
    Task<ExrpApiResponseWrapper<GetLatestPriceResponse>> GetLatestPriceAsync(GetLatestPriceRequest request, GetTokenRequest tokenRequest, CancellationToken cancellationToken);
}
