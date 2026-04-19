using ExchangeRateProvider.Domain.Currencies.Entities;

namespace ExchangeRateProvider.Domain.Markets.Entities
{
    public class MarketCurrency
    {
        public int MarketId { get; set; }
        public int CurrencyId { get; set; }

        public Market Market { get; set; }
        public Currency Currency { get; set; }
    }
}
