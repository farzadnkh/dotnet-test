using ExchangeRateProvider.Domain.Currencies.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.Currencies;

public interface ICurrencyCommandRepository : IBaseCommandRepository<Currency, int, int>
{
}
