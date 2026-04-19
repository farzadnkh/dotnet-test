using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;

public class ProviderApiAccountQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ProviderApiAccountQueryRepository> logger) : BaseQueryRepository<ExchangeRateProviderApiAccount, int>(unitOfWork, logger), IProviderApiAccountQueryRepository
{
    public async Task<IEnumerable<ExchangeRateProviderApiAccount>> GetAllPublishProvidersWithAllIncludesAsync(CancellationToken cancellationToken)
    {
        return await Query.Include(item => item.ExchangeRateProvider)
                .AsNoTracking()
                .Where(item => item.Published)
                .ToListAsync(cancellationToken);
    }

    public async Task<ExchangeRateProviderApiAccount> GetByIdWithAllIncludesAsync(int Id, CancellationToken cancellationToken = default)
    {
        return await Query.Include(item => item.ExchangeRateProvider)
                    .Where(item => item.Id.Equals(Id))
                    .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExchangeRateProviderApiAccount> GetByProviderTypePublishedWithAllIncludesAsync(ProviderType type, CancellationToken cancellationToken = default)
    {
        return await Query.Include(item => item.ExchangeRateProvider)
            .Where(item => item.ExchangeRateProvider.Type.Equals(type))
            .Where(item => item.Published)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IPaginationResponse<ProviderApiAccountResponse>> GetProviderApiAccountsWithPaginationAndFilterAsync(IPaginationRequest<ProviderApiAccountPaginatedFilterRequest> request, CancellationToken cancellationToken = default)
    {
        var query = Query.OrderByDescending(item => item.Id).AsQueryable();
        if (request.Filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.Filter.Owner))
            {
                logger.LogInformation("Applying filter by Owner: {Owner}", request.Filter.Owner);
                query = query.Where(c => c.Owner.Contains(request.Filter.Owner));
            }
            if (request.Filter.Published.HasValue)
            {
                logger.LogInformation("Applying filter by Published: {Published}", request.Filter.Published);
                query = query.Where(c => c.Published.Equals(request.Filter.Published));
            }
            if (request.Filter.ProviderType is not ProviderType.None)
            {
                logger.LogInformation("Applying filter by Type: {Type}", request.Filter.ProviderType);
                query = query.Where(c => c.ExchangeRateProvider.Type.Equals(request.Filter.ProviderType));
            }
        }

        List<ExchangeRateProviderApiAccount> providerApiAccounts = await query
            .Include(item => item.ExchangeRateProvider)
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} categories.", totalCount);

        var paginationResult = new PaginationResponse<ProviderApiAccountResponse>(
            [.. providerApiAccounts.Select(c => new ProviderApiAccountResponse(c.Id, c.Owner, c.ExchangeRateProvider.Type, c.Published, c.ProtocolType, c.Description, c.CreatedOnUtc.ToFormatedDateTime()) {
                EncryptedCredentials = c.Credentials
            })],
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );

        return paginationResult;
    }
}
