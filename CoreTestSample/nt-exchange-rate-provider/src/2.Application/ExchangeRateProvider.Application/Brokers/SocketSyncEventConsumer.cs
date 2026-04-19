using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Domain.Commons.Events;
using MassTransit;

namespace ExchangeRateProvider.Application.Brokers;

public class SocketSyncEventConsumer(IExchangeRateSnapshotMemory snapshotMemory) : IConsumer<SocketSyncMessageArgs>
{
    public async Task Consume(ConsumeContext<SocketSyncMessageArgs> context)
    {
        var message = context.Message;

        switch (message.MessageType)
        {
            case MessageType.ConsumerMarketChanged:
            case MessageType.ConsumerPairChanged:

                if (message.ConsumerId is 0)
                {
                    snapshotMemory.ResetAllCache();
                    break;
                }
                
                snapshotMemory.RemoveAllMemoryForConsumer(message.ConsumerId);
                break;

            case MessageType.ConsumerProviderChanged:
                break;
            default:
                break;
        }

        await Task.CompletedTask;
    }
}
