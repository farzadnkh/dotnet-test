using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders;

public interface IProviderApiAccountQueryRepository : IBaseQueryRepository<ExchangeRateProviderApiAccount, int>
{
    Task<IPaginationResponse<ProviderApiAccountResponse>> GetProviderApiAccountsWithPaginationAndFilterAsync(
    IPaginationRequest<ProviderApiAccountPaginatedFilterRequest> request,
    CancellationToken cancellationToken = default);
    Task<ExchangeRateProviderApiAccount> GetByProviderTypePublishedWithAllIncludesAsync(ProviderType type, CancellationToken cancellationToken = default);
    Task<ExchangeRateProviderApiAccount> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ExchangeRateProviderApiAccount>> GetAllPublishProvidersWithAllIncludesAsync(CancellationToken cancellationToken);
}
