using ExchangeRateProvider.Domain.Markets.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.Markets;

public interface IMarketCommandRepository : IBaseCommandRepository<Market, int, int>
{
}
