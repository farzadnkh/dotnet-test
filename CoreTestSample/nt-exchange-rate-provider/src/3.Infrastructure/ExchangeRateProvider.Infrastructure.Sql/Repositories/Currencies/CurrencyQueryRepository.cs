using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.Currencies.Dtos.Requests;
using ExchangeRateProvider.Contract.Currencies.Dtos.Responses;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Currencies;

public class CurrencyQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<CurrencyQueryRepository> logger) : BaseQueryRepository<Currency, int>(unitOfWork, logger), ICurrencyQueryRepository
{
    public async Task<CurrencyResponse> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
       var currency = await Query.Where(c => c.Code.ToLower().Equals(code.ToLower())).FirstOrDefaultAsync(cancellationToken);

        if (currency is null)
            return null;

        return new CurrencyResponse(
                    currency.Id,
                    currency.Code,
                    currency.Code,
                    currency.Type,
                    currency.CreatedById,
                    currency.Published,
                    [.. currency.Markets.Select(item => item.Id)],
                    currency.LastModifierUserId,
                    currency.CreatedOnUtc,
                    currency.UpdatedOnUtc,
                    currency.DecimalPrecision,
                    currency.Symbol);
    }
    
    public async Task<IPaginationResponse<CurrencyResponse>> GetCurrenciesWithPaginationAndFilterAsync(
        IPaginationRequest<CurrencyPaginatedFilterRequest> request, 
        CancellationToken cancellationToken = default)
    {
        var query = Query.AsQueryable();

        ApplyFilters(ref query, request.Filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedQuery = query.ToPagedQuery((int)request.Paging.Size, request.Paging.Index);

        var currencies = await pagedQuery
            .Include(c => c.Markets)
            .ToListAsync(cancellationToken);

        var mappedItems = MapToCurrencyResponses(currencies);

        return new PaginationResponse<CurrencyResponse>(
            mappedItems,
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );
    }

    private void ApplyFilters(ref IQueryable<Currency> query, CurrencyPaginatedFilterRequest filter)
    {
        if (filter is null) return;

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => c.Name.Contains(filter.Name));

        if (!string.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(c => c.Code.Contains(filter.Code));

        if (filter.Published.HasValue)
            query = query.Where(c => c.Published == filter.Published.Value);

        if (filter.Type.HasValue)
            query = query.Where(c => c.Type == filter.Type.Value);
    }

    private List<CurrencyResponse> MapToCurrencyResponses(List<Currency> currencies)
    {
        return currencies.Select(c => new CurrencyResponse
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            Type = c.Type,
            CreatedById = c.CreatedById,
            Published = c.Published,
            LastModifierUserId = c.LastModifierUserId,
            CreatedOnUtc = c.CreatedOnUtc,
            UpdatedOnUtc = c.UpdatedOnUtc,
            DecimalPrecision = c.DecimalPrecision,
            Symbol = c.Symbol
        }).ToList();
    }
    

    public async Task<CurrencyResponse> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default)
    {
        var currency = await Query
            .Include(item => item.Markets)
            .Include(item => item.MarketCurrencies)
            .Include(item => item.MarketTradingPairs)
            .Where(c => c.Id.Equals(Id))
            .FirstOrDefaultAsync(cancellationToken);

        return new CurrencyResponse(
                    currency.Id,
                    currency.Name,
                    currency.Code,
                    currency.Type,
                    currency.CreatedById,
                    currency.Published,
                    [.. currency.Markets.Select(item => item.Id)],
                    currency.LastModifierUserId,
                    currency.CreatedOnUtc,
                    currency.UpdatedOnUtc,
                    currency.DecimalPrecision,
                    currency.Symbol);
    }
}
