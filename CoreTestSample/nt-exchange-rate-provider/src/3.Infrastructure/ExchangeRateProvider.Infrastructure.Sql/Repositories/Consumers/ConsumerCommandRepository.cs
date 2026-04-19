using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Dtos.Responses;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;
using Microsoft.AspNetCore.Http.Metadata;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Consumers;

public class ConsumerQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ConsumerQueryRepository> logger)
    : BaseQueryRepository<Consumer, int>(unitOfWork, logger), IConsumerQueryRepository
{
    public async Task<Consumer> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await Query.Include(item => item.User)
            .Include(item => item.User)
            .Where(item => item.Apikey.Equals(apiKey))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Consumer> GetByIdWithAllIncludesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Query.Include(item => item.User)
            .Where(item => item.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Consumer> GetConfigurationByConsumerIdWithAllIncludesAsync(int id,
        CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(c => c.User)
            .Include(c => c.ConsumerProviders).ThenInclude(cp => cp.Provider)
            .Include(c => c.ConsumerMarkets).ThenInclude(cm => cm.Market).ThenInclude(m => m.BaseCurrency)
            .Include(c => c.ConsumerPairs).ThenInclude(cp => cp.MarketTradingPair).ThenInclude(tp => tp.Currency)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IPaginationResponse<ConsumerConfigurationResponse>>
        GetConsumersConfigurationWithPaginationAndFilterAsync(
            IPaginationRequest<ConsumerConfigurationPaginatedFilterRequest> request,
            CancellationToken cancellationToken = default)
    {
        var query = Query
            .Include(item => item.User)
            .Include(item => item.ConsumerProviders).ThenInclude(item => item.Provider)
            .Include(item => item.ConsumerMarkets).ThenInclude(item => item.Market)
            .ThenInclude(item => item.BaseCurrency)
            .Include(item => item.ConsumerPairs).ThenInclude(item => item.MarketTradingPair)
            .ThenInclude(item => item.Currency)
            .AsNoTracking()
            .AsQueryable();

        if (request.Filter is not null)
        {
            if (request.Filter.ConsumerId > 0)
            {
                logger.LogInformation("Applying filter by ConsumerId: {ConsumerId}", request.Filter.ConsumerId);
                query = query.Where(c => c.Id.Equals(request.Filter.ConsumerId));
            }

            if (request.Filter.ProviderId > 0)
            {
                logger.LogInformation("Applying filter by ProviderId: {ProviderId}", request.Filter.ProviderId);
                query = query.Where(c => c.ConsumerProviders.Any(item => item.Id == request.Filter.ProviderId));
            }

            if (request.Filter.MarketId > 0)
            {
                logger.LogInformation("Applying filter by MarketId: {MarketId}", request.Filter.MarketId);
                query = query.Where(c => c.ConsumerMarkets.Any(item => item.Id == request.Filter.MarketId));
            }

            if (request.Filter.PairId > 0)
            {
                logger.LogInformation("Applying filter by PairId: {PairId}", request.Filter.PairId);
                query = query.Where(c => c.ConsumerPairs.Any(item => item.Id == request.Filter.PairId));
            }

            if (request.Filter.IsActive.HasValue)
            {
                logger.LogInformation("Applying filter by IsActive: {IsActive}", request.Filter.IsActive);
                query = query.Where(c => c.IsActive.Equals(request.Filter.IsActive));
            }
        }

        List<Consumer> consumers = await query
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} categories.", totalCount);

        var paginationResult = new PaginationResponse<ConsumerConfigurationResponse>(
            [
                .. consumers.Select(c => new ConsumerConfigurationResponse
                {
                    Id = c.Id,
                    ExchangeProviders = [.. c.ConsumerProviders.Select(item => item.Provider)],
                    Markets = [.. c.ConsumerMarkets.Select(item => item.Market)],
                    TradingPairs = [.. c.ConsumerPairs.Select(item => item.MarketTradingPair)],
                    ConsumerId = c.Id,
                    IsActive = c.IsActive,
                    ConsumerUsername = c.User.UserName,
                    SpreadOptions = GetSpreadOptions(c.ConsumerPairs, c.ConsumerMarkets)
                })
            ],
            new BasePaginationResult(totalCount, request.Paging.Index, (int?)request.Paging.Size)
        );

        return paginationResult;
    }

    public async Task<IPaginationResponse<ConsumerResponse>> GetConsumersWithPaginationAndFilterAsync(
        IPaginationRequest<ConsumerPaginatedFilterRequest> request, CancellationToken cancellationToken = default)
    {
        var query = Query.OrderByDescending(item => item.Id).Include(item => item.User).AsQueryable();
        if (request.Filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.Filter.ProjectName))
            {
                logger.LogInformation("Applying filter by Email: {ProjectName}", request.Filter.ProjectName);
                query = query.Where(c => c.ProjectName.Contains(request.Filter.ProjectName));
            }

            if (!string.IsNullOrWhiteSpace(request.Filter.UserName))
            {
                logger.LogInformation("Applying filter by UserName: {UserName}", request.Filter.UserName);
                query = query.Where(c => c.User.NormalizedUserName.Contains(request.Filter.UserName));
            }

            if (request.Filter.IsActive.HasValue)
            {
                logger.LogInformation("Applying filter by IsActive: {IsActive}", request.Filter.IsActive);
                query = query.Where(c => c.IsActive.Equals(request.Filter.IsActive));
            }
        }

        List<Consumer> consumers = await query
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} categories.", totalCount);

        var paginationResult = new PaginationResponse<ConsumerResponse>(
            [
                .. consumers.Select(c => new ConsumerResponse
                {
                    Id = c.Id,
                    UserName = c.User.UserName,
                    Email = c.User.Email,
                    ProjectName = c.ProjectName,
                    CreatedAt = c.CreatedOnUtc.ToFormatedDateTime(),
                    IsActive = c.IsActive
                })
            ],
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );

        return paginationResult;
    }

    public async Task<bool> IsExistApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await Query.AnyAsync(item => item.Apikey.Equals(apiKey), cancellationToken);
    }

    public async Task<bool> IsExistProjectNameAsync(string projectName, CancellationToken cancellationToken = default)
    {
        return await Query.AnyAsync(item => item.ProjectName.ToLower().Equals(projectName.ToLower()), cancellationToken);
    }

    public SpreadOptions GetSpreadOptions(ICollection<ConsumerPair> pairs = null,
        ICollection<ConsumerMarket> markets = null)
    {
        if (pairs is not null)
            return pairs.Select(item => item.SpreadOptions).FirstOrDefault();

        if (markets is not null)
            return markets.Select(item => item.SpreadOptions).FirstOrDefault();

        return null;
    }

    public async Task<IEnumerable<Consumer>> GetAllWithAllIncludesAsync(CancellationToken cancellationToken)
        => await Query.Include(item => item.User).ToListAsync(cancellationToken);

    public async Task<Consumer> GetConsumerWithAllData(
        int consumerId,
        CancellationToken cancellationToken)
    {
        var query = Query.AsNoTracking()
            .Where(cp => cp.Id.Equals(consumerId));

        query = query
            .Include(item => item.ConsumerProviders)
            .ThenInclude(cp => cp.Provider)

            .Include(item => item.ConsumerMarkets.Where(cm => cm.Market.Published))
            .ThenInclude(cm => cm.Market)
            .ThenInclude(m => m.BaseCurrency)

            .Include(item => item.ConsumerMarkets.Where(cm => cm.Market.Published))
            .ThenInclude(cm => cm.Market)
            .ThenInclude(m => m.MarketExchangeRateProviders)

            .Include(item => item.ConsumerPairs.Where(cm => cm.MarketTradingPair.Published && cm.Market.Published))
            .ThenInclude(cp => cp.MarketTradingPair)
            .ThenInclude(mtp => mtp.Currency)

            .Include(item => item.ConsumerPairs.Where(cm => cm.MarketTradingPair.Published && cm.Market.Published))
            .ThenInclude(cp => cp.MarketTradingPair)
            .ThenInclude(mtp => mtp.Market)
            .ThenInclude(m => m.BaseCurrency)

            .Include(item => item.ConsumerPairs.Where(cm => cm.MarketTradingPair.Published && cm.Market.Published))
            .ThenInclude(cp => cp.MarketTradingPair)
            .ThenInclude(mtp => mtp.MarketTradingPairProviders);
        
            var result = await query.FirstOrDefaultAsync(cancellationToken);
        return result;
    }

    public async Task<IEnumerable<ConsumerPair>> GetConsumerAllActivePairs(int consumerId,
        CancellationToken cancellationToken)
    {
        return await Query
            .Include(item => item.ConsumerPairs)
            .ThenInclude(inner => inner.MarketTradingPair)
            .ThenInclude(item => item.Currency)
            .Where(item => item.Id == consumerId && item.IsActive)
            .SelectMany(item => item.ConsumerPairs).ToListAsync(cancellationToken: cancellationToken);
    }

    public  async Task<Consumer> GetReadOnlyConfigurationByConsumerIdWithAllIncludesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Query
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.ConsumerProviders).ThenInclude(cp => cp.Provider)
            .Include(c => c.ConsumerMarkets).ThenInclude(cm => cm.Market).ThenInclude(m => m.BaseCurrency)
            .Include(c => c.ConsumerPairs).ThenInclude(cp => cp.MarketTradingPair).ThenInclude(tp => tp.Currency)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}