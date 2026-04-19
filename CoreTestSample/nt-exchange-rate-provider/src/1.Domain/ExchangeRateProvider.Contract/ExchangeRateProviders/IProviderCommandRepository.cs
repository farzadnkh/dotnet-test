using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface IProviderCommandRepository : IBaseCommandRepository<Provider, int, int>
{
    Task DeleteAllLogics(ICollection<ProviderBusinessLogic> providerBusinessLogics, CancellationToken cancellationToken);
}
