using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class CurrencyExchangeRateCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<CurrencyExchangeRateCommandRepository> logger)
    : BaseCommandRepository<CurrencyExchangeRate, int, int>(unitOfWork, logger), ICurrencyExchangeRateCommandRepository
{
    public async Task UpsertCurrencyExchangeRateFromNewPriceEventAsync(
        NewPriceChangeStreamedMessageArgs @event,
        CancellationToken cancellationToken)
    {
        var existingRate = await Query.FirstOrDefaultAsync(
            x => x.ConsumerId == @event.ConsumerId &&
                 x.MarketTradingPairId == @event.TradingPairId,
            cancellationToken);

        if (existingRate is not null)
        {
            if (existingRate.Buy > @event.PairExchangeRate.Price)
            {
                existingRate.UpdateBuyRate(@event.PairExchangeRate.Price, RateChangeType.Decreased);
                existingRate.UpdateSellRate(@event.PairExchangeRate.Price, RateChangeType.Decreased);
            }
            else if (existingRate.Buy < @event.PairExchangeRate.Price)
            {
                existingRate.UpdateBuyRate(@event.PairExchangeRate.Price, RateChangeType.Increased);
                existingRate.UpdateSellRate(@event.PairExchangeRate.Price, RateChangeType.Increased);
            }

            existingRate.UpdateBuyRate(@event.PairExchangeRate.Price, RateChangeType.Unchanged);
            existingRate.UpdateSellRate(@event.PairExchangeRate.Price, RateChangeType.Unchanged);

            await UpdateAsync(existingRate, cancellationToken, true);
        }
        else
        {
            // Insert new entity
            var newEntity = new CurrencyExchangeRate(
                consumerId: @event.ConsumerId,
                marketTradingPairId: @event.TradingPairId,
                originalRate: @event.PairExchangeRate.Price,
                buy: @event.PairExchangeRate.Price,
                buyRateChange: RateChangeType.Unchanged,
                sell: @event.PairExchangeRate.Price,
                sellRateChange: RateChangeType.Unchanged,
                createdOnUtc: DateTimeOffset.UtcNow
            );

            await AddAsync(newEntity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }
    }
}