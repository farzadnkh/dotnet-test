using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class CurrencyExchangeRateQueryRepository(IUnitOfWork<int> unitOfWork,
    ILogger<CurrencyExchangeRateQueryRepository> logger) : BaseQueryRepository<CurrencyExchangeRate, int>(unitOfWork, logger),ICurrencyExchangeRateQueryRepository
{
    
}