using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Domain.Markets.Entities
{
    public class MarketProvider
    {
        public int MarketId { get; set; }
        public Market Market { get; set; }

        public int ExchangeRateProviderId { get; set; }
        public Provider ExchangeRateProvider { get; set; }
    }
}
