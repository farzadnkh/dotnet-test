using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Domain.Settings.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Settings;

public class SettingCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<SettingCommandRepository> logger) : BaseCommandRepository<Setting, int>(unitOfWork, logger), ISettingCommandRepository
{
}