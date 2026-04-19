using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Models.Responses;

namespace NT.SDK.ExchangeRateProvider.Services.TokenServices;

public interface IExchangeRateProviderTokenService
{
    Task<GetTokenResponse> GetTokenAsync(GetTokenRequest tokenRequest, bool renewToken = false, CancellationToken cancellationToken = default);
}
