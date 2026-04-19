using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using System.Threading.Tasks;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class ProviderCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ProviderCommandRepository> logger) : BaseCommandRepository<Provider, int, int>(unitOfWork, logger), IProviderCommandRepository
{
    public async Task DeleteAllLogics(ICollection<ProviderBusinessLogic> providerBusinessLogics, CancellationToken cancellationToken)
    {
		try
		{
			_unitOfWork.Set<ProviderBusinessLogic>().RemoveRange(providerBusinessLogics);
			await SaveChangesAsync(default);
		}
		catch (Exception)
		{
			throw;
		}
    }
}
