using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface IProviderBusinessLogicQueryRepository : IBaseQueryRepository<ProviderBusinessLogic, int>
{
    Task<IEnumerable<ProviderBusinessLogic>> GetAllByProviderIdAsync(int providerId, CancellationToken cancellationToken);
}
