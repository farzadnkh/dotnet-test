using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Markets;

public class MarketTradingPairQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<MarketTradingPairQueryRepository> logger) : BaseQueryRepository<MarketTradingPair, int>(unitOfWork, logger), IMarketTradingPairQueryRepository
{
    public async Task<IEnumerable<MarketTradingPair>> GetAllPublishedByProviderIdWithAllIncludesAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Query
            .AsNoTracking()
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(item => item.MarketExchangeRateProviders)
            .Include(c => c.Market).ThenInclude(item => item.BaseCurrency)
            .Where(item => item.Published &&
                  (
                      item.MarketTradingPairProviders.Any(p => p.ExchangeRateProviderId == providerId) ||
                      item.Market.MarketExchangeRateProviders.Any(m => m.ExchangeRateProviderId == providerId)
                  ) && item.Currency.Type.Equals(CurrencyType.Crypto)
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MarketTradingPair>> GetAllPublishedByProviderIdsAndMarketIdsWithAllIncludesAsync(IEnumerable<int> providerIds, IEnumerable<int> marketIds, CancellationToken cancellationToken)
    {
        return await Query
            .AsNoTracking()
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(item => item.MarketExchangeRateProviders)
            .Include(c => c.Market).ThenInclude(item => item.BaseCurrency)
            .Where(item => item.Published &&
                           (
                               item.MarketTradingPairProviders.Any(p => providerIds.Contains(p.ExchangeRateProviderId)) ||
                               item.Market.MarketExchangeRateProviders.Any(m => providerIds.Contains(m.ExchangeRateProviderId))
                           ) && item.Currency.Type.Equals(CurrencyType.Crypto) && marketIds.Contains(item.MarketId)
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MarketTradingPair>> GetAllPublishedByProviderIdAndCurrencyTypeWithAllIncludesAsync(int providerId, CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var result = await Query
            .AsNoTracking()
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(item => item.MarketExchangeRateProviders)
            .Include(c => c.Market).ThenInclude(item => item.BaseCurrency)
            .Where(item =>
                         item.Published
                      && item.Currency.Type == currencyType
                      && ((item.Market.RatingMethod == RatingMethod.Automatic && item.RatingMethod != RatingMethod.Manual)
                          || (item.Market.RatingMethod == RatingMethod.Manual && item.RatingMethod != RatingMethod.Manual))
                      && (item.MarketTradingPairProviders.Any(p => p.ExchangeRateProviderId == providerId)
                          || item.Market.MarketExchangeRateProviders.Any(m => m.ExchangeRateProviderId == providerId)))
            .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<IEnumerable<MarketTradingPair>> GetAllPublishedMarketTradingPairsWithIncludesAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(c => c.BaseCurrency)
            .Include(c => c.Market).ThenInclude(item => item.MarketExchangeRateProviders)
            .Where(item => item.Published)
            .ToListAsync(cancellationToken);
    }

    public async Task<MarketTradingPair> GetByIdWithAllIncludesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(c => c.BaseCurrency)
            .Where(item => item.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IPaginationResponse<MarketTradingPairResponse>> GetMarketTradingPairsWithPaginationAndFilterAsync(IPaginationRequest<MarketTradingPairPaginatedFilterRequest> request, CancellationToken cancellationToken = default)
    {
        var query = Query.OrderByDescending(item => item.Id).AsNoTracking().AsQueryable();
        if (request.Filter is not null)
        {
            if (request.Filter.MarketId is not null && request.Filter.MarketId is not 0)
            {
                logger.LogInformation("Applying filter by Name: {MarketId}", request.Filter.MarketId);
                query = query.Where(c => c.MarketId.Equals(request.Filter.MarketId));
            }
            if (request.Filter.CurrencyId is not null && request.Filter.CurrencyId is not 0)
            {
                logger.LogInformation("Applying filter by CurrencyId: {CurrencyId}", request.Filter.CurrencyId);
                query = query.Where(c => c.CurrencyId.Equals(request.Filter.CurrencyId));
            }
            if (request.Filter.Published.HasValue)
            {
                logger.LogInformation("Applying filter by Published: {Published}", request.Filter.Published);
                query = query.Where(c => c.Published.Equals(request.Filter.Published));
            }
        }

        List<MarketTradingPair> markettradingpairs = await query
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(c => c.BaseCurrency)
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} categories.", totalCount);

        var paginationResult = new PaginationResponse<MarketTradingPairResponse>(
            [.. markettradingpairs.Select(c => new MarketTradingPairResponse
            {
                Id = c.Id,
                Currency = $"{c.Currency.Name} ({c.Currency.Code})",
                Pair = $"{c.Currency.Code}/{c.Market.BaseCurrency.Code}",
                Published = c.Published,
                Description = c.Description,
                SpreadOptions = c.SpreadOptions,
            })],
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );

        return paginationResult;
    }

    public Task<MarketTradingPair> GetByCurrencyCodeAndMarketAsync(string currencyCode, int marketId, CancellationToken cancellationToken = default)
    {
        return Query.
            Include(item => item.Currency)
            .FirstOrDefaultAsync(item =>
                   item.Currency.Code.Equals(currencyCode) &&
                   item.MarketId.Equals(marketId),
            cancellationToken);
    }

    public async Task<List<MarketTradingPair>> GetAllManualTradingParisAsync(CancellationToken cancellationToken = default)
    {
        var result = await Query
            .AsNoTracking()
            .Include(c => c.MarketTradingPairProviders).ThenInclude(item => item.ExchangeRateProvider)
            .Include(c => c.Currency)
            .Include(c => c.Market).ThenInclude(item => item.MarketExchangeRateProviders)
            .Include(c => c.Market).ThenInclude(item => item.BaseCurrency)
            .Where(item =>
                         item.Published &&
                          ((item.Market.RatingMethod == RatingMethod.Automatic && item.RatingMethod == RatingMethod.Manual)
                          || (item.Market.RatingMethod == RatingMethod.Manual && item.RatingMethod == RatingMethod.Manual)))
            .ToListAsync(cancellationToken);
        return result;
    }
}
