using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using MassTransit;

namespace ExchangeRateProvider.Infrastructure.Sql.Commons.Notifications;

public class Notifier(
    IPublishEndpoint publishEndpoint,
    IChannelRegistry channelRegistry) : INotifier
{
    public async Task NotifyNewPriceChangeStreamedAsync(NewPriceChangeStreamedMessageArgs message) => await publishEndpoint.Publish(message);

    public async Task NotifyPriceChangedAsync(PriceChangedEventMessageArgs message)
       => await publishEndpoint.Publish(message);

    public async Task SyncBackgroundJobsAsync(BackgroundJobSyncMessageArgs message)
        => await channelRegistry.PublishAsync(message);

    public async Task SyncClientSideSocketAsync(SocketSyncMessageArgs message)
     => await publishEndpoint.Publish(message);

    public async Task SyncJobsIntervalAsync(JobsIntervalMessageArgs message)
     => await channelRegistry.PublishAsync(message);

    public async Task SyncSettingsAsync(SettingModel setting)
        => await publishEndpoint.Publish(setting);
}
