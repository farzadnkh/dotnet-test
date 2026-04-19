using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Contract.Consumers.Services;

public interface IConsumerPairAggregator
{
    public Task<(IEnumerable<MarketTradingPair>, IEnumerable<ConsumerProvider>)> GetExchangeRatePairsForConsumerAsync(int consumerId, GetExchangeRateForAllPairsWithFilter filter = null, CancellationToken cancellationToken = default);
}