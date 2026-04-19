using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Contract.Settings.Services;
using MassTransit;

namespace ExchangeRateProvider.Application.Brokers;

public class SyncSettingsEventConsumer(ISettingService settingService) : IConsumer<SettingModel>
{
    public Task Consume(ConsumeContext<SettingModel> context)
    {
        settingService.UpSertSettingInMemory(context.Message);
        return Task.CompletedTask;
    }
}
