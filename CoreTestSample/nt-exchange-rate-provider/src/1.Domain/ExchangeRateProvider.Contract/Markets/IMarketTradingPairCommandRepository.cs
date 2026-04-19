using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.Markets;

public interface IMarketTradingPairCommandRepository : IBaseCommandRepository<MarketTradingPair, int, int>
{
    Task BulkUpdateRatingMethodBaseOnMarketRatingMethodAsync(int marketId, RatingMethod ratingMethod, CancellationToken cancellationToken);
    Task BulkUpdateProviderBaseOnMarketRatingMethodAsync(int marketId, List<int> providerId,CancellationToken cancellationToken);
}
