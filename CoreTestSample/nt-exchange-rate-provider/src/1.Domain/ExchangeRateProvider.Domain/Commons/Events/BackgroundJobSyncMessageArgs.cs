using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Domain.Commons.Events;

public class BackgroundJobSyncMessageArgs
{
    public ProviderType ProviderType { get; set; }
}
