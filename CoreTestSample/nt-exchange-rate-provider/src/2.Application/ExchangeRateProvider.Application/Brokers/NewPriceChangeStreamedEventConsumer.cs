using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.Commons.Events;
using MassTransit;

namespace ExchangeRateProvider.Application.Brokers;

public class NewPriceChangeStreamedEventConsumer(ICurrencyExchangeRateCommandRepository repository) : IConsumer<NewPriceChangeStreamedMessageArgs>
{
    public async Task Consume(ConsumeContext<NewPriceChangeStreamedMessageArgs> context)
    {
        await repository.UpsertCurrencyExchangeRateFromNewPriceEventAsync(context.Message, context.CancellationToken);
    }
}