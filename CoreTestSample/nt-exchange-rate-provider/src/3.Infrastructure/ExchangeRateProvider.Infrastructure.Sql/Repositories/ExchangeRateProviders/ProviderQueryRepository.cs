using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class ProviderQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ProviderQueryRepository> logger) : BaseQueryRepository<Provider, int>(unitOfWork, logger), IProviderQueryRepository
{
    public async Task<IEnumerable<Provider>> GetAllPublishedProvidersAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(c => c.ProviderBusinessLogics)
                  .Where(item => item.Published)
                  .ToListAsync(cancellationToken);
    }

    public async Task<Provider> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(item => item.ProviderBusinessLogics)
            .Include(item => item.ApiAccounts)
            .Where(item => item.Id.Equals(Id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProviderResponse> GetByTypePublishedAsync(ProviderType type, CancellationToken cancellationToken = default)
    {
        var provider = await Query
                          .Where(item => item.Type.Equals(type) && item.Published)
                          .FirstOrDefaultAsync(cancellationToken);
        
        if(provider is null ) return null;

        return new(provider.Id, provider.Name, provider.Type, provider.Published, null);
    }

    public async Task<Provider> GetByTypePublishedWithAllIncludesAsync(ProviderType type, CancellationToken cancellationToken = default)
    {
         return await Query
                  .Include(item => item.ApiAccounts)
                  .Include(item => item.ProviderBusinessLogics)
                  .Where(item => item.Type.Equals(type) && item.Published)
                  .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IPaginationResponse<ProviderResponse>> GetProvidersWithPaginationAndFilterAsync(IPaginationRequest<ProviderPaginatedFilterRequest> request, CancellationToken cancellationToken = default)
    {
        var query = Query.OrderByDescending(q => q.Id).AsQueryable();
        if (request.Filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.Filter.Name))
            {
                logger.LogInformation("Applying filter by Name: {Name}", request.Filter.Name);
                query = query.Where(c => c.Name.Contains(request.Filter.Name));
            }
            if (request.Filter.Published.HasValue)
            {
                logger.LogInformation("Applying filter by Published: {Published}", request.Filter.Published);
                query = query.Where(c => c.Published.Equals(request.Filter.Published));
            }
            if (request.Filter.ProviderType is not ProviderType.None)
            {
                logger.LogInformation("Applying filter by Type: {Type}", request.Filter.ProviderType);
                query = query.Where(c => c.Type.Equals(request.Filter.ProviderType));
            }
        }

        List<Provider> providers = await query
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} categories.", totalCount);

        var paginationResult = new PaginationResponse<ProviderResponse>(
            [.. providers.Select(c => new ProviderResponse(c.Id, c.Name, c.Type, c.Published, c.ProviderBusinessLogics.Select(cc => cc.Name).ToList()))],
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );
        
        return paginationResult;
    }

    public async Task<Provider> GetByNamePublishedWithAllIncludesAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(item => item.ApiAccounts)
            .Include(item => item.ProviderBusinessLogics)
            .Where(item => item.Name.Equals(name) && item.Published)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
