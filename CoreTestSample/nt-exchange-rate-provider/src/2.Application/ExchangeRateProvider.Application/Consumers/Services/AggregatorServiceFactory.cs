using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.Markets.Entities;
using System.Runtime.CompilerServices;

namespace ExchangeRateProvider.Application.Consumers.Services;

public class AggregatorServiceFactory(
    IConsumerPairAggregator consumerPairAggregator,
    IPairAggregator pairAggregator,
    IExchangeRateSnapshotMemory snapshotMemory,
    INotifier notifier)
    : IAggregatorServiceFactory
{
    public async IAsyncEnumerable<PairExchangeRateDto> GetPairExchangeRatesForConsumerAllPairsAsync(
        int consumerId,
        GetExchangeRateForAllPairsWithFilter filter,
        bool shouldApplyNewFilter,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var (result, pairSnapshots) = snapshotMemory.TryToGetConsumerAllPairs(consumerId);
        if (!result || shouldApplyNewFilter)
        {
            var (pairs, consumerProviders) = await consumerPairAggregator.GetExchangeRatePairsForConsumerAsync(consumerId, filter, ct);
            var consumerProvidersList = consumerProviders.Select(i => i.ProviderId).ToList();
            snapshotMemory.RemoveAllMemoryForConsumer(consumerId);
            foreach (var pair in pairs)
            {
                var snapshot = pair.ToSnapshot(consumerProvidersList);
                snapshotMemory.SetTradingPairSnapShot(consumerId, pair.Id, snapshot);
                var dto = await pairAggregator.GetPairExchangeRate(consumerId, snapshot);
                if (dto is not null && dto.Price > 0)
                {
                    snapshotMemory.SetPairExchangeRateSnapShot(consumerId, snapshot.Id, dto);
                    await notifier.NotifyNewPriceChangeStreamedAsync(new NewPriceChangeStreamedMessageArgs(dto, consumerId, pair.Id));
                    yield return dto;
                }

                yield return null;
            }
        }
        else
        {
            foreach (var snapshot in pairSnapshots)
            {
                var (lookUpResult, _) = snapshotMemory.TryToGetTradingPairSnapShot(consumerId, snapshot.Id);
                if (lookUpResult) yield return null;
                var newData = await pairAggregator.GetPairExchangeRate(consumerId, snapshot);
                if (newData is not null && newData.Price > 0)
                {
                    snapshotMemory.SetPairExchangeRateSnapShot(consumerId, snapshot.Id, newData);
                    await notifier.NotifyNewPriceChangeStreamedAsync(new NewPriceChangeStreamedMessageArgs(newData, consumerId, snapshot.Id));
                    yield return newData;
                }
                yield return null;
            }
        }
    }

}

