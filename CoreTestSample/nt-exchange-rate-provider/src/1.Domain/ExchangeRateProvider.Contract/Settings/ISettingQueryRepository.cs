using ExchangeRateProvider.Domain.Settings.Entities;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.Settings;

public interface ISettingQueryRepository : IBaseQueryRepository<Setting, int>
{
}
