using NT.SDK.ExchangeRateProvider.Models.Requests;

namespace NT.SDK.ExchangeRateProvider.Services.StreamRateServices
{
    public interface IExchangeRateProviderStreamRateService
    {
        Task StartStreamAsync(GetTokenRequest getTokenRequest, GetLatestPriceRequest priceRequest = null, bool renewToken = false, CancellationToken cancellationToken = default);
        Task SendMessageAsync(GetLatestPriceRequest priceRequest, CancellationToken cancellationToken = default);
        Task ReconnectAsync(GetTokenRequest getTokenRequest, CancellationToken cancellationToken = default);
    }
}
