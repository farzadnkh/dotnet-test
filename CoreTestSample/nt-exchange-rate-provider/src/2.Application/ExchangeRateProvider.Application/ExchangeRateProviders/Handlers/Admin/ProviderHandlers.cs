using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Application.ExchangeRateProviders.Handlers.Admin
{
    public static class ProviderHandlers
    {
        public static async Task<IPaginationResponse<ProviderResponse>> GetAllWithPaginationAsync(
            [FromQuery] int? index,
            [FromQuery] RequestPagingSize? size,
            [AsParameters] ProviderPaginatedFilterRequest providerPaginatedFilterRequest,
            IProviderQueryRepository repo,
            ILogger<Provider> logger)
        {
            try
            {
                PaginationRequest<ProviderPaginatedFilterRequest> request = new()
                {
                    Filter = providerPaginatedFilterRequest,
                    Paging = new()
                    {
                        Index = index ?? 0,
                        Size = size ?? 0
                    }
                };

                var result = await repo.GetProvidersWithPaginationAndFilterAsync(request, default);
                logger.LogInformation("Successfully retrieved {Count} providers.", result.Base.Total);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving providers with pagination and filter.");
                throw ApplicationBadRequestException.Create("An error occurred while retrieving providers.");
            }
        }

        public static async Task<ProviderResponse> GetByIdAsync(
            [FromRoute] int id,
            IProviderQueryRepository repo,
            ILogger<Provider> logger)
        {
            try
            {
                if (id == 0)
                    throw ApplicationBadRequestException.Create("The provider ID is required.");

                var provider = await repo.GetByIdWithAllIncludesAsync(id) ?? throw ApplicationNotFoundException.Create($"Provider with ID: {id} not found.");
                var selectedMarkets = provider.ProviderBusinessLogics.Select(p => p.Name).ToList();

                logger.LogInformation("Provider with ID {Id} successfully retrieved.", id);
                return new(provider.Id, provider.Name, provider.Type, provider.Published, selectedMarkets);
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request for provider with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Provider with ID {Id} not found: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while retrieving provider with ID {Id}.", id);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving the provider.");
            }
        }

        public static async Task<ResponseWrapper<ProviderResponse>> CreateAsync(
            [FromBody] CreateProviderRequest request,
            IProviderCommandRepository repo,
            IProviderQueryRepository queryrepo,
            ILogger<Provider> logger)
        {
            try
            {
                if (request.ProviderType is ProviderType.None)
                    return new ResponseWrapper<ProviderResponse>(["Provider type is required."]);

                if (string.IsNullOrWhiteSpace(request.Name))
                    return new ResponseWrapper<ProviderResponse>(["Provider name is required."]);
                
                var existProvider = await queryrepo.GetByTypePublishedAsync(request.ProviderType, default);

                if (existProvider != null)
                {
                    return new ResponseWrapper<ProviderResponse>(["Provider is exist."]);
                }

                var provider = Provider.Create(
                    request.Name,
                    request.ProviderType,
                    request.CreatedById,
                    request.Published);
                
                await repo.AddAsync(provider);
                var id = await repo.SaveChangesAsync(default);

                foreach (var selectedMarketName in request.SelectedMarkets)
                    provider.AddProviderBusinessLogics(selectedMarketName, request.CreatedById);

                await repo.UpdateAsync(provider, default, true);

                var response = new ProviderResponse(
                    id,
                    provider.Name,
                    provider.Type,
                    provider.Published,
                    null);

                logger.LogInformation("Provider created successfully with ID {Id}.", id);
                return new ResponseWrapper<ProviderResponse>(response);
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while creating provider: {Message}", ex.Message);
                return new ResponseWrapper<ProviderResponse>([$"Bad request while creating provider: {ex.Message}"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while creating the provider.");
                return new ResponseWrapper<ProviderResponse>([
                    $"An unexpected error occurred while creating the provider. {ex.Message}"
                ]);
            }
        }

        public static async Task<ResponseWrapper<ProviderResponse>> UpdateAsync(
            [FromBody] CreateProviderRequest request,
            [FromRoute] int id,
            IProviderCommandRepository repo,
            IProviderQueryRepository queryrepo,
            INotifier notifier,
            ILogger<Provider> logger)
        {
            try
            {
                if (request.ProviderType is ProviderType.None)
                    return new ResponseWrapper<ProviderResponse>(["Provider type is required."]);

                if (string.IsNullOrWhiteSpace(request.Name))
                    return new ResponseWrapper<ProviderResponse>(["Provider name is required."]);

                var provider = await queryrepo.GetByIdWithAllIncludesAsync(id, default);

                if (provider == null)
                {
                    return new ResponseWrapper<ProviderResponse>([$"Provider with ID: {id} not found."]);
                }

                if(provider.Type != request.ProviderType)
                {
                    logger.LogWarning("Illegal Provider Type modification attempt. ProviderId: {ProviderId}, OriginalType: {OriginalType}, AttemptedType: {AttemptedType}",
                                id,
                                provider.Type,
                                request.ProviderType);

                    return new ResponseWrapper<ProviderResponse>([$"Provider Type cannot be modified Code: {request.ProviderType}."]);
                }

                if (!request.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var existProvider = await queryrepo.GetByNamePublishedWithAllIncludesAsync(request.Name, default);

                    if (existProvider != null)
                    {
                        return new ResponseWrapper<ProviderResponse>(["Provider is exist."]);
                    }
                }

                await repo.DeleteAllLogics(provider.ProviderBusinessLogics, default);
                provider.RemoveAllLogics();

                if(request.SelectedMarkets is not null && request.ProviderType == ProviderType.CryptoCompare)
                {
                    foreach (var selectedMarketName in request.SelectedMarkets)
                        provider.AddProviderBusinessLogics(selectedMarketName, request.CreatedById);
                }

                provider.Update(request.Name, request.ProviderType, request.Published, request.CreatedById);

                var result = await repo.UpdateAsync(provider, request.CreatedById, withSaveChanged: true);

                if (result)
                {
                    var response = new ProviderResponse(
                        id,
                        provider.Name,
                        provider.Type,
                        provider.Published,
                        null);

                    await notifier.SyncBackgroundJobsAsync(new()
                    {
                        ProviderType = provider.Type
                    });

                    logger.LogInformation("Provider updated successfully with ID {Id}.", id);
                    return new ResponseWrapper<ProviderResponse>(response);
                }

                logger.LogWarning("Failed to update provider with ID {Id}.", id);
                return null;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while updating provider with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating provider with ID {Id}.", id);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while updating the provider.");
            }
        }
    }
}