using System.Threading.Channels;
using ExchangeRateProvider.Contract.Commons;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRateProvider.Application.Commons.Channels;

public sealed class ChannelRegistry(IServiceProvider serviceProvider) : IChannelRegistry
{
    public ValueTask PublishAsync<T>(T message)
    {
        try
        {
            return GetWriter<T>().WriteAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
        }
        return ValueTask.CompletedTask;
    }

    public bool Publish<T>(T message) => GetWriter<T>().TryWrite(message);

    public ChannelReader<T> GetReader<T>() => GetChannel<T>().Reader;
    private ChannelWriter<T> GetWriter<T>() => GetChannel<T>().Writer;
    public bool TryComplete<T>() => GetWriter<T>().TryComplete();
    private Channel<T> GetChannel<T>() => serviceProvider.GetRequiredService<Channel<T>>();
}
