using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface ICurrencyExchangeRateQueryRepository : IBaseQueryRepository<CurrencyExchangeRate, int>;