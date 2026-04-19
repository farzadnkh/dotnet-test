using ExchangeRateProvider.Domain.Settings.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.Settings;

public interface ISettingCommandRepository : IBaseCommandRepository<Setting, int>
{

}
