using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Application.Consumers.Extensions;

public readonly ref struct SpreadOptionStruct(
    ConsumerPair consumerPair,
    ConsumerMarket consumerMarket,
    TradingPairSnapshot tradingPair,
    Market market)
{
    public SpreadOptions GetActiveSpreadOption()
    {
        if (consumerPair?.SpreadOptions is { SpreadEnabled: true })
            return consumerPair.SpreadOptions;

        if (consumerMarket?.SpreadOptions is { SpreadEnabled: true })
            return consumerMarket.SpreadOptions;

        if (tradingPair.SpreadOptions is { SpreadEnabled: true })
            return tradingPair.SpreadOptions;

        return market?.SpreadOptions is { SpreadEnabled: true } ? market.SpreadOptions : null;
    }
}
