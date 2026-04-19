using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.Markets;

public interface IMarketTradingPairQueryRepository : IBaseQueryRepository<MarketTradingPair, int>
{
    Task<IPaginationResponse<MarketTradingPairResponse>> GetMarketTradingPairsWithPaginationAndFilterAsync(
        IPaginationRequest<MarketTradingPairPaginatedFilterRequest> request,
        CancellationToken cancellationToken = default);
    Task<MarketTradingPair> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarketTradingPair>> GetAllPublishedMarketTradingPairsWithIncludesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<MarketTradingPair>> GetAllPublishedByProviderIdWithAllIncludesAsync(int providerId, CancellationToken cancellationToken);
    Task<IEnumerable<MarketTradingPair>> GetAllPublishedByProviderIdAndCurrencyTypeWithAllIncludesAsync(int providerId, CurrencyType currencyType, CancellationToken cancellationToken);

    Task<IEnumerable<MarketTradingPair>> GetAllPublishedByProviderIdsAndMarketIdsWithAllIncludesAsync(
        IEnumerable<int> providerIds, IEnumerable<int> marketIds, CancellationToken cancellationToken);

    Task<MarketTradingPair> GetByCurrencyCodeAndMarketAsync(string currencyCode, int marketId, CancellationToken cancellationToken = default);
    Task<List<MarketTradingPair>> GetAllManualTradingParisAsync(CancellationToken cancellationToken = default);
}
