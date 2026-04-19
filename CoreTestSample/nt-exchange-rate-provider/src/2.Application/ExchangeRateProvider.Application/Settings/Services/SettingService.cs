using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Contract.Settings.Services;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Application.Settings.Handlers.Admin;

namespace ExchangeRateProvider.Application.Settings.Services;

public class SettingService : ISettingService
{
    private SettingModel _currentSettings;
    public int SnapshotClockTime { get; set; }

    public async Task<SettingModel> LoadSettingsAsync(ISettingQueryRepository settingQueryRepository)
    {
        if(_currentSettings == null)
        {
            _currentSettings = await SettingHandler.GetSetting(settingQueryRepository, default);
            SnapshotClockTime = _currentSettings.SnapshotClockTime;
            return _currentSettings;
        }

        return _currentSettings;
    }

    public void UpSertSettingInMemory(SettingModel newSettings)
    {
        _currentSettings = newSettings;
        SnapshotClockTime = newSettings.SnapshotClockTime;
    }
}
