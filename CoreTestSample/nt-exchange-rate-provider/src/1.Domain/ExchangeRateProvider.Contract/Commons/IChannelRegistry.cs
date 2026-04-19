using System.Threading.Channels;

namespace ExchangeRateProvider.Contract.Commons;

public interface IChannelRegistry
{
    ValueTask PublishAsync<T>(T message);
    bool Publish<T>(T message);
    ChannelReader<T> GetReader<T>();
    bool TryComplete<T>();
}
