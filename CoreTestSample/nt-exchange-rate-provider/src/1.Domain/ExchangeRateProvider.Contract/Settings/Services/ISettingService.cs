using ExchangeRateProvider.Contract.Settings.Dtos;

namespace ExchangeRateProvider.Contract.Settings.Services;

public interface ISettingService
{
    int SnapshotClockTime { get; }
    Task<SettingModel> LoadSettingsAsync(ISettingQueryRepository settingQueryRepository);
    void UpSertSettingInMemory(SettingModel newSettings);
}
