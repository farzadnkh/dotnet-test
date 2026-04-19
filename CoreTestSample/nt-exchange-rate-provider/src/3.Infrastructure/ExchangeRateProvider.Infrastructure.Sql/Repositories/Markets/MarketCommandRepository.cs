using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Markets;

public class MarketCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<MarketCommandRepository> logger) : BaseCommandRepository<Market, int, int>(unitOfWork, logger), IMarketCommandRepository
{
}
