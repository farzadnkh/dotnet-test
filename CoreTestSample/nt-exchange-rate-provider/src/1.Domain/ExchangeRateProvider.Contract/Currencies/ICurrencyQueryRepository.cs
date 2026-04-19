using ExchangeRateProvider.Contract.Currencies.Dtos.Requests;
using ExchangeRateProvider.Contract.Currencies.Dtos.Responses;
using ExchangeRateProvider.Domain.Currencies.Entities;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.Currencies;

public interface ICurrencyQueryRepository : IBaseQueryRepository<Currency, int>
{
    Task<IPaginationResponse<CurrencyResponse>> GetCurrenciesWithPaginationAndFilterAsync(
        IPaginationRequest<CurrencyPaginatedFilterRequest> request,
        CancellationToken cancellationToken = default);

    Task<CurrencyResponse> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<CurrencyResponse> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default);
}

