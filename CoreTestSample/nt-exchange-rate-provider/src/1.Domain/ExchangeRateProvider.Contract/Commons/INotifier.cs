using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;

namespace ExchangeRateProvider.Contract.Commons;

public interface INotifier : ISingletonLifetime
{
    Task NotifyNewPriceChangeStreamedAsync(NewPriceChangeStreamedMessageArgs message);
    Task NotifyPriceChangedAsync(PriceChangedEventMessageArgs message);
    Task SyncBackgroundJobsAsync(BackgroundJobSyncMessageArgs message);
    Task SyncClientSideSocketAsync(SocketSyncMessageArgs message);
    Task SyncSettingsAsync(SettingModel setting);
    Task SyncJobsIntervalAsync(JobsIntervalMessageArgs message);
}
