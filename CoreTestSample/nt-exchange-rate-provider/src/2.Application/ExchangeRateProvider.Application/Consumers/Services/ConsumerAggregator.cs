using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Application.Consumers.Services;

public class ConsumerAggregator(
    IConsumerQueryRepository consumerQueryRepository,
    IMarketTradingPairQueryRepository marketTradingPairQueryRepository) : IConsumerPairAggregator
{
    public async Task< (IEnumerable<MarketTradingPair>, IEnumerable<ConsumerProvider>)> GetExchangeRatePairsForConsumerAsync(int consumerId,
        GetExchangeRateForAllPairsWithFilter filter, CancellationToken ct)
    {
        var consumer = await consumerQueryRepository.GetConsumerWithAllData(consumerId, ct);
        if (consumer.ConsumerPairs.Count > 0)
        {
            var tradingPairs = consumer.ConsumerPairs.Select(i => i.MarketTradingPair).ToList();
            return (ApplyFilter(filter, tradingPairs), consumer.ConsumerProviders.ToList());
        }

        if (consumer.ConsumerMarkets.Count > 0 && consumer.ConsumerProviders.Count > 0)
        {
            var tradingPairs =
                await marketTradingPairQueryRepository.GetAllPublishedByProviderIdsAndMarketIdsWithAllIncludesAsync(
                    consumer.ConsumerProviders.Select(i => i.ProviderId),
                    consumer.ConsumerMarkets.Select(i => i.MarketId), ct);
            return (ApplyFilter(filter, tradingPairs), consumer.ConsumerProviders.ToList());
        }

        if (consumer.ConsumerProviders.Count <= 0)
            throw new ApplicationException("Consumer config not implemented, at least it should contain providers");
        {
            var allProviderPairs = await Task.WhenAll(
                consumer.ConsumerProviders.Select(p =>
                    marketTradingPairQueryRepository.GetAllPublishedByProviderIdWithAllIncludesAsync(p.ProviderId, ct)));

            var flattenedPairs = allProviderPairs.SelectMany(pairs => pairs);
            return (ApplyFilter(filter, flattenedPairs), consumer.ConsumerProviders.ToList());
        }

    }

    private IEnumerable<MarketTradingPair> ApplyFilter(GetExchangeRateForAllPairsWithFilter filter,
        IEnumerable<MarketTradingPair> pairs)
    {
        var query = pairs.AsQueryable();
        if (filter == null) return query.ToList();
        
        if (filter.Markets?.Count > 0)
        {
            query = query.Where(i => filter.Markets.Contains(i.Market.BaseCurrency.Code));
        }

        if (filter.Pairs?.Count > 0)
        {
            query = query.Where(i => filter.Pairs.Contains(i.Currency.Code));
        }

        if (filter.ProviderTypes != ProviderType.None)
        {
            query = query.Where(i => i.MarketTradingPairProviders.Any(mtp => mtp.ExchangeRateProvider.Type == filter.ProviderTypes));
        }
        return query.ToList();
    }
}