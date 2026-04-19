using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Domain.Markets.Entities;

public class MarketTradingPairProvider
{
    public int MarektTradingPairId { get; set; }
    public MarketTradingPair MarketTradingPair { get; set; }

    public int ExchangeRateProviderId { get; set; }
    public Provider ExchangeRateProvider { get; set; }
}
