using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface ICurrencyExchangeRateCommandRepository : IBaseCommandRepository<CurrencyExchangeRate, int, int>
{
    public Task UpsertCurrencyExchangeRateFromNewPriceEventAsync(NewPriceChangeStreamedMessageArgs @event, CancellationToken cancellationToken);
}