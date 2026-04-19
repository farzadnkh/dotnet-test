using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Domain.Commons.Events;

public class SocketSyncMessageArgs
{
    public int ConsumerId { get; set; }
    public MessageType MessageType { get; set; }
    public ProviderType Provider { get; set; }
}

public enum MessageType
{
    ConsumerMarketChanged,
    ConsumerPairChanged,
    ConsumerProviderChanged
}
