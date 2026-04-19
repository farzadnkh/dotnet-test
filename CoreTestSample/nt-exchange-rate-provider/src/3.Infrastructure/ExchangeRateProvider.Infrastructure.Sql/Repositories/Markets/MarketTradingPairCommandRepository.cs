using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Markets;

public class MarketTradingPairCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<MarketTradingPairCommandRepository> logger) : BaseCommandRepository<MarketTradingPair, int, int>(unitOfWork, logger), IMarketTradingPairCommandRepository
{
    public async Task BulkUpdateProviderBaseOnMarketRatingMethodAsync(int marketId, List<int> providerId, CancellationToken cancellationToken)
    {
        var paris = await Query
            .Include(item => item.MarketTradingPairProviders)
            .Where(item => item.MarketId == marketId).ToListAsync(cancellationToken);

        foreach (var pair in paris)
        {
            var includedProviderId = pair.MarketTradingPairProviders.Select(x => x.ExchangeRateProviderId).AsEnumerable();
            var res = providerId.AsEnumerable().Except(includedProviderId);
            if(res.Count() > 0)
            {
                pair.SetExchangeRateProviders([.. res]);
                await UpdateAsync(pair, cancellationToken, false);
            }
        }
    }

    public async Task BulkUpdateRatingMethodBaseOnMarketRatingMethodAsync(int marketId, RatingMethod ratingMethod, CancellationToken cancellationToken)
    {
        await _dbSet
            .Where(item => item.MarketId == marketId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.RatingMethod, e => ratingMethod), cancellationToken);
    }
}
