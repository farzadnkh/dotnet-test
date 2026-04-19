using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Markets.Entities;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.Markets;

public interface IMarketQueryRepository : IBaseQueryRepository<Market, int>
{
    Task<IPaginationResponse<MarketResponse>> GetMarketsWithPaginationAndFilterAsync(
    IPaginationRequest<MarketPaginatedFilterRequest> request,
    CancellationToken cancellationToken = default);
    Task<Market> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default);

    Task<List<Market>> GetByIdWithAllIncludesAsync(List<int> ids, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Market>> GetAllPublishedMarketsWithIncludesAsync(CancellationToken cancellationToken = default);
    Task<List<Market>> GetAllMarketsWithIncludesAsync(CancellationToken cancellationToken = default);

    Task<bool> IsExistAsync(int currencyId, CancellationToken cancellationToken);

    Task<IEnumerable<Market>> GetAllMarketsByProviderIdAsync(int providerId, CancellationToken cancellationToken);
}
