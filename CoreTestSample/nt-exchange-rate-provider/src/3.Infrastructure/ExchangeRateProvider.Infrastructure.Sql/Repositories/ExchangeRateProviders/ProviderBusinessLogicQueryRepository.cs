using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class ProviderBusinessLogicQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ProviderBusinessLogicQueryRepository> logger) : BaseQueryRepository<ProviderBusinessLogic, int>(unitOfWork, logger), IProviderBusinessLogicQueryRepository
{
    public async Task<IEnumerable<ProviderBusinessLogic>> GetAllByProviderIdAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Query
            .Where(item => item.ProviderId.Equals(providerId))
            .ToListAsync(cancellationToken);
    }
}
