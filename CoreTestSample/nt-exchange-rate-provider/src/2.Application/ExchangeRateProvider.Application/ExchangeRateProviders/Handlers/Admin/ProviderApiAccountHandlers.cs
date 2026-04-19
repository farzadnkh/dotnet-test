using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Application.ExchangeRateProviders.Handlers.Admin;

public static class ProviderApiAccountHandlers
{
    public static async Task<IPaginationResponse<ProviderApiAccountResponse>> GetAllWithPaginationAsync(
        [FromQuery] int? index,
        [FromQuery] RequestPagingSize? size,
        [AsParameters] ProviderApiAccountPaginatedFilterRequest providerApiAccountPaginatedFilterRequest,
        IProviderApiAccountQueryRepository repo,
        ILogger<ExchangeRateProviderApiAccount> logger)
    {
        logger.LogInformation(
            "Attempting to retrieve ProviderApiAccounts with pagination. Index: {Index}, Size: {Size}, Filter: {@ProviderApiAccountPaginatedFilterRequest}",
            index, size, providerApiAccountPaginatedFilterRequest);

        try
        {
            PaginationRequest<ProviderApiAccountPaginatedFilterRequest> request = new()
            {
                Filter = providerApiAccountPaginatedFilterRequest,
                Paging = new()
                {
                    Index = index ?? 0,
                    Size = size ?? RequestPagingSize.Five
                }
            };

            var result = await repo.GetProviderApiAccountsWithPaginationAndFilterAsync(request, default);

            logger.LogInformation("Successfully retrieved {Count} ProviderApiAccounts. Total items: {TotalCount}", result.Items.Count(), result.Base.Total);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving ProviderApiAccounts. Request Index: {Index}, Size: {Size}, Filter: {@ProviderApiAccountPaginatedFilterRequest}",
                            index, size, providerApiAccountPaginatedFilterRequest);
            throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving Provider API accounts. Please try again later or contact support.", ex);
        }
    }

    public static async Task<ProviderApiAccountResponse> GetByIdAsync(
        [FromRoute] int id,
        IProviderApiAccountQueryRepository repo,
        ILogger<ExchangeRateProviderApiAccount> logger)
    {
        logger.LogInformation("Attempting to retrieve ProviderApiAccount with ID: {Id}", id);

        try
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid ID provided for GetByIdAsync: {Id}", id);
                throw ApplicationBadRequestException.Create("A valid ProviderApiAccount ID is required.");
            }

            var providerApiAccount = await repo.GetByIdWithAllIncludesAsync(id, default)
                ?? throw ApplicationNotFoundException.Create($"ProviderApiAccount with ID: {id} not found.");

            ProviderApiAccountResponse result = new(providerApiAccount.Id,
                                    providerApiAccount.Owner,
                                    providerApiAccount.ExchangeRateProvider.Type,
                                    providerApiAccount.Published,
                                    providerApiAccount.ProtocolType,
                                    providerApiAccount.Description,
                                    providerApiAccount.CreatedOnUtc.ToFormatedDateTime())
            {
                EncryptedCredentials = providerApiAccount.Credentials
            };

            logger.LogInformation("ProviderApiAccount with ID {Id} successfully retrieved.", id);
            return result;
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Bad request for ProviderApiAccount ID {Id}.", id);
            throw ApplicationBadRequestException.Create(string.IsNullOrWhiteSpace(ex.Message) ? $"Bad request when trying to retrieve ProviderApiAccount with ID {id}." : ex.Message, ex);
        }
        catch (ApplicationNotFoundException ex)
        {
            logger.LogWarning(ex, "ProviderApiAccount not found with ID {Id}.", id);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving ProviderApiAccount with ID {Id}.", id);
            throw ApplicationBadRequestException.Create($"An unexpected error occurred while retrieving ProviderApiAccount with ID {id}.", ex);
        }
    }

    public static async Task<ResponseWrapper<ProviderApiAccountResponse>> CreateAsync(
        [FromBody] CreateProviderApiAccountRequest request,
        IProviderApiAccountCommandRepository commandRepo,
        IProviderQueryRepository providerQueryRepository,
        INotifier notifier,
        ILogger<ExchangeRateProviderApiAccount> logger)
    {
        logger.LogInformation("Attempting to create ProviderApiAccount with request: {@CreateProviderApiAccountRequest}", request);

        try
        {
            request.ValidateCreateRequest();

            var provider = await providerQueryRepository.GetByTypePublishedAsync(request.ProviderType, default);

            if (provider == null)
            {
                return new ResponseWrapper<ProviderApiAccountResponse>([$"Provider with Type: {request.ProviderType} not found. Cannot create ProviderApiAccount."]);
            }
            
            var providerApiAccount = new ExchangeRateProviderApiAccount(
                provider.Id,
                request.Owner,
                request.ProtocolType,
                request.Credentials,
                request.Description,
                request.CreatedById,
                request.Published);

            await commandRepo.AddAsync(providerApiAccount);
            await commandRepo.SaveChangesAsync(providerApiAccount.CreatedById, default);
            var newId = providerApiAccount.Id;

            if (providerApiAccount.Published)
            {
                await notifier.SyncBackgroundJobsAsync(new BackgroundJobSyncMessageArgs
                {
                    ProviderType = provider.Type
                });
            }

            var response = new ProviderApiAccountResponse(
                newId,
                providerApiAccount.Owner,
                provider.Type,
                providerApiAccount.Published,
                providerApiAccount.ProtocolType,
                providerApiAccount.Description,
                providerApiAccount.CreatedOnUtc.ToFormatedDateTime());

            logger.LogInformation("ProviderApiAccount created successfully with ID {Id}.", newId);
            return new ResponseWrapper<ProviderApiAccountResponse>(response);
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Bad request while creating ProviderApiAccount. Request: {@CreateProviderApiAccountRequest}", request);
            return new ResponseWrapper<ProviderApiAccountResponse>([$"Bad request while creating ProviderApiAccount."]);
        }
        catch (ApplicationNotFoundException ex)
        {
            logger.LogWarning(ex, "Not found exception while creating ProviderApiAccount. Request: {@CreateProviderApiAccountRequest}", request);
            return new ResponseWrapper<ProviderApiAccountResponse>([$"Bad request while creating ProviderApiAccount."]);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating ProviderApiAccount. Request: {@CreateProviderApiAccountRequest}", request);
            return new ResponseWrapper<ProviderApiAccountResponse>([$"Bad request while creating ProviderApiAccount."]);
        }
    }

    public static async Task<ResponseWrapper<ProviderApiAccountResponse>> UpdateAsync(
        [FromBody] CreateProviderApiAccountRequest request,
        [FromRoute] int id,
        IProviderApiAccountCommandRepository commandRepo,
        IProviderApiAccountQueryRepository queryRepository,
        IProviderQueryRepository providerQueryRepository,
        INotifier notifier,
        ILogger<ExchangeRateProviderApiAccount> logger)
    {
        logger.LogInformation("Attempting to update ProviderApiAccount with ID: {Id}. Request: {@UpdateProviderApiAccountRequest}", id, request);

        try
        {
            request.ValidateUpdateRequest(id);

            if (id <= 0)
            {
                logger.LogWarning("Invalid ID {Id} provided for UpdateAsync.", id);
                throw ApplicationBadRequestException.Create("A valid ProviderApiAccount ID is required for update.");
            }

            var providerApiAccount = await queryRepository.GetByIdAsync(id, default)
                ?? throw ApplicationNotFoundException.Create($"ProviderApiAccount with ID: {id} not found. Update failed.");

            var provider = await providerQueryRepository.GetByTypePublishedAsync(request.ProviderType, default)
                ?? throw ApplicationNotFoundException.Create($"Provider with Type: {request.ProviderType} not found. Cannot update ProviderApiAccount.");

            if(providerApiAccount.ProviderId != provider.Id)
            {
                logger.LogWarning("Illegal Provider Type modification attempt. ProviderId: {ProviderId}, OriginalType: {OriginalType}, AttemptedType: {AttemptedType}",
                    id,
                    provider.Type,
                    request.ProviderType);

                throw ApplicationNotFoundException.Create($"Provider Type cannot be modified Code: {request.ProviderType}.");
            }

            providerApiAccount.Update(
                provider.Id,
                request.Owner,
                request.ProtocolType,
                request.Credentials,
                request.Description,
                request.CreatedById,
                request.Published);


            var updatedSuccessfully = await commandRepo.UpdateAsync(providerApiAccount, request.CreatedById, withSaveChanged: true);

            if (updatedSuccessfully)
            {
                if (providerApiAccount.Published)
                {
                    await notifier.SyncBackgroundJobsAsync(new()
                    {
                        ProviderType = provider.Type
                    });
                }

                var response = new ProviderApiAccountResponse(
                    providerApiAccount.Id,
                    providerApiAccount.Owner,
                    provider.Type,
                    providerApiAccount.Published,
                    providerApiAccount.ProtocolType,
                    providerApiAccount.Description,
                    providerApiAccount.CreatedOnUtc.ToFormatedDateTime());

                logger.LogInformation("ProviderApiAccount updated successfully with ID {Id}.", id);
                return new()
                {
                    Response = response,
                    IsSuccess = true,
                };
            }
            else
            {
                logger.LogWarning("Failed to update ProviderApiAccount with ID {Id}. The update operation did not report success.", id);
                throw ApplicationBadRequestException.Create($"Failed to update ProviderApiAccount with ID {id}. The operation did not complete successfully.");
            }
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Bad request while updating ProviderApiAccount with ID {Id}. Request: {@UpdateProviderApiAccountRequest}", id, request);
            throw ApplicationBadRequestException.Create(string.IsNullOrWhiteSpace(ex.Message) ? $"Bad request when updating ProviderApiAccount with ID {id}." : ex.Message, ex);
        }
        catch (ApplicationNotFoundException ex)
        {
            logger.LogWarning(ex, "Not found exception during update of ProviderApiAccount with ID {Id}. Request: {@UpdateProviderApiAccountRequest}", id, request);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating ProviderApiAccount with ID {Id}. Request: {@UpdateProviderApiAccountRequest}", id, request);
            throw ApplicationBadRequestException.Create($"An unexpected error occurred while updating ProviderApiAccount with ID {id}.", ex);
        }
    }

    private static void ValidateCreateRequest(this CreateProviderApiAccountRequest request)
    {
        if (request.ProviderType is ProviderType.None)
            throw ApplicationBadRequestException.Create("ProviderType is required.");

        if (string.IsNullOrWhiteSpace(request.Owner))
            throw ApplicationBadRequestException.Create("Owner Name is required.");

        if (request.ProtocolType == ProtocolType.None)
            throw ApplicationBadRequestException.Create("ProtocolType is required.");

        if (request.CreatedById <= 0)
            throw ApplicationBadRequestException.Create("A valid CreatedById is required.");

        if (request.Credentials == null)
            throw ApplicationBadRequestException.Create("Credentials are required.");
    }

    private static void ValidateUpdateRequest(this CreateProviderApiAccountRequest request, int entityId)
    {
        if (entityId <= 0)
            throw ApplicationBadRequestException.Create("A valid ProviderApiAccount ID is required for update.");

        if (request.ProviderType is ProviderType.None)
            throw ApplicationBadRequestException.Create("ProviderType is required for update.");

        if (string.IsNullOrWhiteSpace(request.Owner))
            throw ApplicationBadRequestException.Create("Owner Name is required for update.");

        if (request.ProtocolType == ProtocolType.None)
            throw ApplicationBadRequestException.Create("ProtocolType is required for update.");
    }

    public static byte[] SetCredentials(this ProviderApiAccountCredentials credentials, string encryptionKey = null)
    {
        var stringCredentials = JsonConvert.SerializeObject(credentials);
        try
        {
            return CryptoHelper.Encrypt(stringCredentials, encryptionKey);
        }
        catch (Exception ex)
        {
            throw ApplicationBadRequestException.Create("Failed to secure credentials. Please check the input data.", ex);
        }
    }

    public static ProviderApiAccountCredentials GetCredentials(this byte[] credentials, string encryptionKey = null)
    {
        try
        {
            var decryptedCredentials = CryptoHelper.Decrypt(credentials, encryptionKey);
            return JsonConvert.DeserializeObject<ProviderApiAccountCredentials>(decryptedCredentials);
        }
        catch (JsonException ex)
        {
            throw ApplicationBadRequestException.Create("Failed to read credentials data after decryption. Data may be corrupt.", ex);
        }
        catch (Exception ex)
        {
            throw ApplicationBadRequestException.Create("Failed to decrypt credentials. Key may be incorrect or data corrupt.", ex);
        }
    }
}
