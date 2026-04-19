using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.Settings.Entities;
using ExchangeRateProvider.Domain.Settings.Enums;

namespace ExchangeRateProvider.Application.Settings.Handlers.Admin;

public static class SettingHandler
{
    public static async Task<Setting> GetSettingByNameAsync(
        SettingName settingName, 
        ISettingQueryRepository settingQueryRepository, 
        CancellationToken cancellationToken)
    {
        var result = await settingQueryRepository.GetAsync(s => s.Name == settingName.ToString(), cancellationToken);
        return result.LastOrDefault();
    }

    public static async Task<SettingModel> GetSetting(
        ISettingQueryRepository settingQueryRepository, 
        CancellationToken cancellationToken)
    {
        var settings = await settingQueryRepository.GetAllAsync(cancellationToken);
        var setttingGroup = settings.GetSettings();

        var model = new SettingModel
        {
            SnapshotClockTime = setttingGroup.SnapshotClockTimeSetting == null ? 0 : int.Parse(setttingGroup.SnapshotClockTimeSetting.Value),
            PercentageDifference = setttingGroup.PercentageDifference == null ? 0 : int.Parse(setttingGroup.PercentageDifference.Value),
            TimeDifference = setttingGroup.TimeDifference == null ? 0 : int.Parse(setttingGroup.TimeDifference.Value),
            CryptoJobsInterval = setttingGroup.CryptoJobsInterval == null ? 5 : int.Parse(setttingGroup.CryptoJobsInterval.Value),
            FiatJobsInterval = setttingGroup.FiatJobsInterval == null ? 1 : int.Parse(setttingGroup.FiatJobsInterval.Value),
            ManualRateChangePercentage = setttingGroup.ManualRateChangePercentage == null ? 1 : int.Parse(setttingGroup.ManualRateChangePercentage.Value),
        };

        return model;
    }

    public static async Task<bool> Save(
        SettingModel model,
        ISettingQueryRepository settingQueryRepository,
        ISettingCommandRepository settingCommandRepository,
        INotifier notifier,
        CancellationToken cancellationToken)
    {
        model.ValidateModel();

        var settings = await settingQueryRepository.GetAllAsync(cancellationToken);
        var setttingGroup = settings.GetSettings();

        var snapshotClockTimeSetting = setttingGroup.SnapshotClockTimeSetting;
        var percentageDifference = setttingGroup.PercentageDifference;
        var timeDifference = setttingGroup.TimeDifference;
        var cryptoJobsInterval = setttingGroup.CryptoJobsInterval;
        var fiatJobInterval =setttingGroup.FiatJobsInterval;
        var manualRateChangePercentage = setttingGroup.ManualRateChangePercentage;
        

        bool intervalsHasChanged = false;
        JobsIntervalMessageArgs jobIntervalMessage = new();

        if (snapshotClockTimeSetting == null)
        {
            await settingCommandRepository.AddAsync(new Setting(SettingName.SnapshotClockTime.ToString(), model.SnapshotClockTime.ToString()), cancellationToken);
        }
        else
        {
            snapshotClockTimeSetting.Value = model.SnapshotClockTime.ToString();
            await settingCommandRepository.UpdateAsync(snapshotClockTimeSetting, cancellationToken);
        }

        if (percentageDifference == null)
        {
            await settingCommandRepository.AddAsync(new Setting(SettingName.PercentageDifference.ToString(), model.PercentageDifference.ToString()), cancellationToken);
        }
        else
        {
            percentageDifference.Value = model.PercentageDifference.ToString();
            await settingCommandRepository.UpdateAsync(percentageDifference, cancellationToken);
        }

        if (timeDifference == null)
        {
            await settingCommandRepository.AddAsync(new Setting(SettingName.TimeDifference.ToString(), model.TimeDifference.ToString()), cancellationToken);
        }
        else
        {
            timeDifference.Value = model.TimeDifference.ToString();
            await settingCommandRepository.UpdateAsync(timeDifference, cancellationToken);
        }

        if (cryptoJobsInterval == null)
        {
            await settingCommandRepository.AddAsync(new Setting(SettingName.CryptoJobsInterval.ToString(), model.CryptoJobsInterval.ToString()), cancellationToken);
        }
        else
        {
            if (!model.CryptoJobsInterval.ToString().Equals(cryptoJobsInterval.Value))
            {
                cryptoJobsInterval.Value = model.CryptoJobsInterval.ToString();
                await settingCommandRepository.UpdateAsync(cryptoJobsInterval, cancellationToken);

                intervalsHasChanged = true;
                jobIntervalMessage.IntervalTypes = IntervalTypes.Crypto;
                jobIntervalMessage.CryptoJobInterval = model.CryptoJobsInterval;
            }
        }

        if (fiatJobInterval == null)
            await settingCommandRepository.AddAsync(new Setting(SettingName.FiatJobsInterval.ToString(), model.FiatJobsInterval.ToString()), cancellationToken);
        else
        {
            if (!model.FiatJobsInterval.ToString().Equals(fiatJobInterval.Value))
            {
                fiatJobInterval.Value = model.FiatJobsInterval.ToString();
                await settingCommandRepository.UpdateAsync(fiatJobInterval, cancellationToken);

                jobIntervalMessage.FiatsJobInterval = model.FiatJobsInterval;

                intervalsHasChanged = true;
                if (jobIntervalMessage.IntervalTypes == IntervalTypes.Crypto)
                    jobIntervalMessage.IntervalTypes = IntervalTypes.All;
                else
                    jobIntervalMessage.IntervalTypes = IntervalTypes.Fiats;
            }
        }

        if (manualRateChangePercentage == null)
        {
            await settingCommandRepository.AddAsync(new Setting(SettingName.ManualRateChangePercentage.ToString(), model.ManualRateChangePercentage.ToString()), cancellationToken);
        }
        else
        {
            manualRateChangePercentage.Value = model.ManualRateChangePercentage.ToString();
            await settingCommandRepository.UpdateAsync(manualRateChangePercentage, cancellationToken);
        }

        await settingCommandRepository.SaveChangesAsync(cancellationToken);
        await notifier.SyncSettingsAsync(model);

        if (intervalsHasChanged)
            await notifier.SyncJobsIntervalAsync(jobIntervalMessage);

        return true;
    }

    public static void ValidateModel(this SettingModel model)
    {
        if (model == null) throw ApplicationBadRequestException.Create("There is an  error occurred while saving the Settings.");
        if (model.SnapshotClockTime < 0) throw ApplicationBadRequestException.Create("Snapshot Clock Time cannot be negative.");
        if (model.PercentageDifference < 0) throw ApplicationBadRequestException.Create("Percentage Difference Time cannot be negative.");
        if (model.TimeDifference < 0) throw ApplicationBadRequestException.Create("TimeDifference cannot be negative.");
        if (model.CryptoJobsInterval <= 0) throw ApplicationBadRequestException.Create("Crypto Jobs Interval cannot be negative or zero.");
        if (model.FiatJobsInterval <= 0) throw ApplicationBadRequestException.Create("Fiat Jobs Interval cannot be negative or zero.");
        if (model.ManualRateChangePercentage <= 0) throw ApplicationBadRequestException.Create("ManualRateChangePercentage cannot be negative or zero.");
    }

    private static SettingGroup GetSettings(this IEnumerable<Setting> settings)
        => new()
        {
            SnapshotClockTimeSetting = settings.LastOrDefault(s => s.Name == SettingName.SnapshotClockTime.ToString()),
            PercentageDifference = settings.LastOrDefault(s => s.Name == SettingName.PercentageDifference.ToString()),
            TimeDifference = settings.LastOrDefault(s => s.Name == SettingName.TimeDifference.ToString()),
            CryptoJobsInterval = settings.LastOrDefault(s => s.Name == SettingName.CryptoJobsInterval.ToString()),
            FiatJobsInterval = settings.LastOrDefault(s => s.Name == SettingName.FiatJobsInterval.ToString()),
            ManualRateChangePercentage = settings.LastOrDefault(s => s.Name == SettingName.ManualRateChangePercentage.ToString())
        };

    public class SettingGroup
    {
        public Setting SnapshotClockTimeSetting { get; set; }
        public Setting PercentageDifference { get; set; }
        public Setting TimeDifference { get; set; }
        public Setting CryptoJobsInterval { get; set; }
        public Setting FiatJobsInterval { get; set; }
        public Setting ManualRateChangePercentage { get; set; }
    }
}