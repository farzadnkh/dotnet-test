using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface IProviderQueryRepository : IBaseQueryRepository<Provider, int>
{
    Task<IPaginationResponse<ProviderResponse>> GetProvidersWithPaginationAndFilterAsync(
    IPaginationRequest<ProviderPaginatedFilterRequest> request,
    CancellationToken cancellationToken = default);

    Task<ProviderResponse> GetByTypePublishedAsync(ProviderType type, CancellationToken cancellationToken = default);

    Task<Provider> GetByTypePublishedWithAllIncludesAsync(ProviderType type, CancellationToken cancellationToken = default);

    Task<Provider> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Provider>> GetAllPublishedProvidersAsync(CancellationToken cancellationToken = default);

    Task<Provider> GetByNamePublishedWithAllIncludesAsync(string name, CancellationToken cancellationToken = default);
}
