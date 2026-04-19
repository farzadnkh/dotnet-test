using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Domain.Settings.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Settings;

public class SettingQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<SettingQueryRepository> logger) : BaseQueryRepository<Setting, int>(unitOfWork, logger), ISettingQueryRepository
{
}