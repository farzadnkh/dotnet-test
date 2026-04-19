using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Domain.Commons.Events
{
    public class PriceChangedEventMessageArgs
    {
        public required ProviderType Provider { get; init; }
        public required string Pair { get; init; }
        public required decimal Price { get; init; }
        public required decimal Volume { get; init; }
        public required string Market { get; init; }
        public required long Ticks { get; init; }
    }
}
