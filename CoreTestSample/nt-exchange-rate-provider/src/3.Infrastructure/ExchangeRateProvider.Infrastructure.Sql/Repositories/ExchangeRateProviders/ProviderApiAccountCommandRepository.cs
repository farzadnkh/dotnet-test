using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class ProviderApiAccountCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ProviderApiAccountCommandRepository> logger) : BaseCommandRepository<ExchangeRateProviderApiAccount, int, int>(unitOfWork, logger), IProviderApiAccountCommandRepository
{
}
