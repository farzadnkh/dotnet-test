using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.Commons.Services
{
    public interface IRedisService
    {
        Task SavePairsPriceDataToRedisAsync(ProviderType provider, string pair, decimal price, decimal volume, string market = "");
    }
}
