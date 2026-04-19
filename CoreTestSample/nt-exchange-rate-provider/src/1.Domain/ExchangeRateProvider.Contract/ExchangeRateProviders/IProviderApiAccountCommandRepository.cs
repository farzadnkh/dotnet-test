using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface IProviderApiAccountCommandRepository : IBaseCommandRepository<ExchangeRateProviderApiAccount, int, int>
{
}
