using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class ProviderBusinessLogicCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ProviderBusinessLogicCommandRepository> logger) : BaseCommandRepository<ProviderBusinessLogic, int, int>(unitOfWork, logger), IProviderBusinessLogicCommandRepository
{
}
