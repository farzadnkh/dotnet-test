using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Domain.Commons.Events;
using MassTransit;

namespace ExchangeRateProvider.Application.Brokers;

public class PriceChangedEventConsumer(IExchangeRateSnapshotMemory snapshotMemory) : IConsumer<PriceChangedEventMessageArgs>
{
    public async Task Consume(ConsumeContext<PriceChangedEventMessageArgs> context)
    {
        var message = context.Message;
        var targetPair = message.Pair.ToUpperInvariant();
        
        foreach (var (consumerId, tradingPairs) in snapshotMemory.GetAllConsumersTradingPairs())
        {
            var targetPairSnapshot = tradingPairs.FirstOrDefault(tp =>
                $"{tp.CurrencyCode}{tp.BaseCurrencyCode}".Equals(targetPair, StringComparison.OrdinalIgnoreCase));

            snapshotMemory.RemoveExchangeRateSnapShot(consumerId, targetPairSnapshot.Id);
        }
        await Task.CompletedTask;
    }
}
