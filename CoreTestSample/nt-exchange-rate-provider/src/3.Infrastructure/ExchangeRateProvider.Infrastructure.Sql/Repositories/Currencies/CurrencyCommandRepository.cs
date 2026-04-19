using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Domain.Currencies.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Currencies;

public class CurrencyCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<CurrencyCommandRepository> logger) : BaseCommandRepository<Currency, int, int>(unitOfWork, logger), ICurrencyCommandRepository
{
}
