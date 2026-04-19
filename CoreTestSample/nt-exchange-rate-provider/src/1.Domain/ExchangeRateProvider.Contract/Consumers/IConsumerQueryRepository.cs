using ExchangeRateProvider.Domain.Consumers.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.Consumers;

public interface IConsumerCommandRepository : IBaseCommandRepository<Consumer, int>
{
    Task DeleteAllConfigurations(ICollection<ConsumerMarket> consumerMarkets, ICollection<ConsumerPair> consumerPairs, ICollection<ConsumerProvider> consumerProviders);
}
