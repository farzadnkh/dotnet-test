using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Markets;

public class MarketQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<MarketQueryRepository> logger) : BaseQueryRepository<Market, int>(unitOfWork, logger), IMarketQueryRepository
{
    public async  Task<IEnumerable<Market>> GetAllMarketsByProviderIdAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Query
            .AsNoTracking()
            .Include(item => item.BaseCurrency)
            .Include(item => item.MarketExchangeRateProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Where(item => item.MarketExchangeRateProviders.Any(p => p.ExchangeRateProviderId.Equals(providerId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Market>> GetAllMarketsWithIncludesAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(item => item.BaseCurrency)
            .Include(item => item.MarketExchangeRateProviders).ThenInclude(item => item.ExchangeRateProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Market>> GetAllPublishedMarketsWithIncludesAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(item => item.BaseCurrency)
            .Include(item => item.TradingPairs)
            .Include(item => item.MarketExchangeRateProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Where(item => item.Published)
            .ToListAsync(cancellationToken);
    }

    public async Task<Market> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default)
    {
        return await Query
                .Include(item => item.BaseCurrency)
                .Include(item => item.MarketExchangeRateProviders).ThenInclude(item => item.ExchangeRateProvider)
                .Where(item => item.Id.Equals(Id))
                .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<List<Market>> GetByIdWithAllIncludesAsync(List<int> ids, CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(item => item.BaseCurrency)
            .Include(item => item.MarketExchangeRateProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Where(item => ids.Contains(item.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IPaginationResponse<MarketResponse>> GetMarketsWithPaginationAndFilterAsync(IPaginationRequest<MarketPaginatedFilterRequest> request, CancellationToken cancellationToken = default)
    {
        var query = Query.OrderByDescending(item => item.Id).AsNoTracking().AsQueryable();
        if (request.Filter is not null)
        {
            if (request.Filter.CurrencyId is not null && request.Filter.CurrencyId is not 0)
            {
                logger.LogInformation("Applying filter by CurrencyId: {CurrencyId}", request.Filter.CurrencyId);
                query = query.Where(c => c.BaseCurrencyId.Equals(request.Filter.CurrencyId));
            }
            if (request.Filter.MarketCalculationTerm is not null)
            {
                logger.LogInformation("Applying filter by MarketCalculationTerm: {MarketCalculationTerm}", request.Filter.MarketCalculationTerm);
                query = query.Where(c => c.CalculationTerm.Equals(request.Filter.MarketCalculationTerm));
            }
            if (request.Filter.Published.HasValue)
            {
                logger.LogInformation("Applying filter by Published: {Published}", request.Filter.Published);
                query = query.Where(c => c.Published.Equals(request.Filter.Published));
            }
        }

        List<Market> markets = await query
            .Include(c => c.MarketExchangeRateProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.BaseCurrency)
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} categories.", totalCount);

        var paginationResult = new PaginationResponse<MarketResponse>(
            [.. markets.Select(c => new MarketResponse
            {
                Id = c.Id,
                Published = c.Published,
                BaseCurrencyCode = c.BaseCurrency.Code,
                BaseCurrencyId = c.BaseCurrencyId,
                BaseCurrencyName = c.BaseCurrency.Name,
                IsDefault = c.IsDefault,
                Term = c.CalculationTerm,
                ExchangeProviders = [.. c.MarketExchangeRateProviders.Select(item => item.ExchangeRateProvider)],
                Name = c.BaseCurrency.GetCurrencyName(),
                RatingMethod = c.RatingMethod
            })],
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );

        return paginationResult;
    }

    public async Task<bool> IsExistAsync(int currencyId, CancellationToken cancellationToken)
        => await Query.AnyAsync(item => item.BaseCurrencyId.Equals(currencyId), cancellationToken);
}
