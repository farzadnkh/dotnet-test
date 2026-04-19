using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Contract.Consumers.Dtos;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Dtos.Responses;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace ExchangeRateProvider.Application.Consumers.Handlers.Admin
{
    public static class ConsumerHandlers
    {
        public static async Task<IPaginationResponse<ConsumerResponse>> GetAllWithPaginationAsync(
            [FromQuery] int? index,
            [FromQuery] RequestPagingSize? size,
            [AsParameters] ConsumerPaginatedFilterRequest consumerPaginatedFilterRequest,
            IConsumerQueryRepository repo,
            ILogger<Consumer> logger)
        {
            try
            {
                logger.LogInformation(
                    "Attempting to retrieve all consumers with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                    index, size, JsonConvert.SerializeObject(consumerPaginatedFilterRequest));

                PaginationRequest<ConsumerPaginatedFilterRequest> request = new()
                {
                    Filter = consumerPaginatedFilterRequest,
                    Paging = new()
                    {
                        Index = index ?? 0,
                        Size = size ?? 0
                    }
                };

                var result = await repo.GetConsumersWithPaginationAndFilterAsync(request, default);
                logger.LogInformation("Successfully retrieved {Count} consumers.", result.Base.Total);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex,
                    "Error occurred while retrieving consumers with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                    index, size, JsonConvert.SerializeObject(consumerPaginatedFilterRequest));
                throw ApplicationBadRequestException.Create("Error occurred while retrieving consumers.",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<ConsumerResponse> GetByIdAsync(
            [FromRoute] int id,
            IConsumerQueryRepository repo,
            ILogger<Consumer> logger)
        {
            try
            {
                logger.LogInformation("Attempting to retrieve consumer with ID: {Id}", id);

                if (id <= 0)
                {
                    logger.LogWarning("Invalid consumer ID provided: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                var consumer = await repo.GetByIdWithAllIncludesAsync(id);

                if (consumer == null)
                {
                    logger.LogWarning("Consumer with ID {Id} not found.", id);
                    throw ApplicationNotFoundException.Create($"Consumer with ID: {id} not found.");
                }

                logger.LogInformation("Consumer with ID {Id} successfully retrieved.", id);
                return new()
                {
                    Id = consumer.Id,
                    IsActive = consumer.IsActive,
                    CreatedAt = consumer.CreatedOnUtc.ToFormatedDateTime(),
                    Email = consumer.User?.Email,
                    UserName = consumer.User?.UserName,
                    ProjectName = consumer.ProjectName,
                    FirstName = consumer.User?.FirstName,
                    LastName = consumer.User?.LastName,
                };
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request for consumer ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Consumer not found with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving consumer with ID {Id}.", id);
                throw ApplicationBadRequestException.Create(
                    "An unexpected error occurred while retrieving the consumer.",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<ConsumerResponse> CreateAsync(
            [FromBody] CreateConsumerRequest request,
            IConsumerCommandRepository repo,
            UserManager<User> userManager,
            IApiKeyClientService apiKeyClientService,
            EncryptionConfiguration encryptionConfiguration,
            ILogger<Consumer> logger)
        {
            try
            {
                logger.LogInformation(
                    "Attempting to create a new consumer for project: {ProjectName}, Email: {Email}, UserName: {UserName}",
                    request.ProjectName, request.Email, request.UserName);

                request.ValidateCreateRequest();

                var isEmailExist = await userManager.FindByEmailAsync(request.Email);
                if (isEmailExist is not null)
                {
                    logger.LogWarning("Consumer creation failed: Duplicate email '{Email}' is not acceptable.",
                        request.Email);
                    return new()
                    {
                        IsSuccess = false,
                        ErrorMessage = "Duplicate Email is not acceptable."
                    };
                }

                var isExistUserName = await userManager.FindByNameAsync(request.UserName);
                if (isExistUserName is not null)
                {
                    logger.LogWarning("Consumer creation failed: Duplicate username '{UserName}' is not acceptable.",
                        request.UserName);
                    return new()
                    {
                        IsSuccess = false,
                        ErrorMessage = "Duplicate Username is not acceptable."
                    };
                }

                var user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    NormalizedEmail = request.Email.Normalize(),
                    NormalizedUserName = request.UserName.Normalize(),
                    EmailConfirmed = true,
                };

                var identityUserResult = await userManager.CreateAsync(user);

                if (identityUserResult.Succeeded)
                {
                    logger.LogInformation("User created successfully for consumer: {UserName}", user.UserName);

                    var consumer = Consumer.Create(user.Id, request.ProjectName, request.CreatedById);

                    await repo.AddAsync(consumer);
                    var id = await repo.SaveChangesAsync(request.CreatedById, default);

                    var response = new ConsumerResponse(
                        consumer.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.UserName,
                        consumer.ProjectName,
                        consumer.IsActive,
                        consumer.CreatedOnUtc.ToFormatedDateTime());

                    logger.LogInformation("Consumer created successfully with ID {Id}.", id);
                    return response;
                }
                else
                {
                    var errors = string.Join(", ", identityUserResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create user for consumer: {Errors}", errors);
                    throw ApplicationBadRequestException.Create(
                        $"Could not create consumer. User creation failed: {errors}");
                }
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while creating consumer: {ErrorMessage}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating consumer. Request: {Request}",
                    JsonConvert.SerializeObject(request));
                throw ApplicationBadRequestException.Create("An unexpected error occurred while creating the consumer.",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<ResponseWrapper<ConsumerResponse>> UpdateAsync(
            [FromBody] CreateConsumerModel request,
            [FromRoute] int id,
            IConsumerCommandRepository commandRepository,
            IConsumerQueryRepository queryRepository,
            IMarketQueryRepository marketQueryRepository,
            IMarketTradingPairQueryRepository tradingPairQueryRepository,
            UserManager<User> userManager,
            IRedisDatabase redisDatabase,
            INotifier notifier,
            ConfigurationDbContext configurationDb,
            string encryptionKey,
            ILogger<Consumer> logger)
        {
            try
            {
                logger.LogInformation("Attempting to update consumer with ID: {Id}. Request: {Request}", id,
                    JsonConvert.SerializeObject(request));

                if (request.TokenTtl <= 0)
                    return new ResponseWrapper<ConsumerResponse>(["Token Time To Live should be greater than 0"]);

                if (id <= 0)
                {
                    logger.LogWarning("Invalid consumer ID provided for update: {Id}", id);
                    return new ResponseWrapper<ConsumerResponse>([$"Invalid consumer ID provided for update: {id}"]);
                }

                var consumer = await queryRepository.GetByIdWithAllIncludesAsync(id);
                if (consumer == null)
                {
                    logger.LogWarning("Consumer with ID {Id} not found for update.", id);
                    return new ResponseWrapper<ConsumerResponse>([$"Consumer with ID: {id} not found."]);
                }

                if (consumer.User.Email != request.Email)
                {
                    var isEmailExist = await userManager.FindByEmailAsync(request.Email);
                    if (isEmailExist is not null && isEmailExist.Id != consumer.User.Id)
                    {
                        logger.LogWarning("Update failed: Duplicate email '{Email}' already exists for another user.", request.Email);
                        return new ResponseWrapper<ConsumerResponse>(["Duplicate Email is not acceptable."]);
                    }
                }

                if (consumer.User.UserName != request.UserName)
                {
                    var isExistUserName = await userManager.FindByNameAsync(request.UserName);
                    if (isExistUserName is not null && isExistUserName.Id != consumer.User.Id)
                    {
                        logger.LogWarning("Update failed: Duplicate username '{UserName}' already exists for another user.", request.UserName);
                        return new ResponseWrapper<ConsumerResponse>(["Duplicate UserName is not acceptable."]);
                    }
                }

                consumer.Update(request.ProjectName, request.IsActive);
                consumer.SetWhiteListIps(request.WhiteListIps);
                consumer.LastModifierUserId = request.CreatorUserId;

                await commandRepository.UpdateAsync(consumer, default, true);


                var providerIds = request.ProviderIds;
                var marketIds = request.MarketIds;
                var pairIds = request.PairIds;

                var consumerConfigurationRequest = new CreateConsumerConfigurationRequest(
                    ConsumerId: request.ConsumerId,
                    IsActive: request.IsActive,
                    SpreadOptions: request.SpreadOptions,
                    CreatedById: request.CreatorUserId,
                    ExchangeRateProviderIds: providerIds,
                    MarketIds: marketIds,
                    TradingPairIds: pairIds
                );


                await ConsumerConfigurationHandlers
                    .UpdateAsync(consumerConfigurationRequest, commandRepository, queryRepository, tradingPairQueryRepository, marketQueryRepository, notifier, logger);

                if (request.TokenTtl is not 0)
                    await UpdateClientIdLifeTime(request, configurationDb, encryptionKey, consumer);

                if (!consumer.IsActive)
                    await redisDatabase.SetAddAsync(RedisKeys.DeActivatedConsumers(), consumer.Id);

                if (consumer.IsActive)
                    if (await redisDatabase.SetContainsAsync(RedisKeys.DeActivatedConsumers(), consumer.Id))
                        await redisDatabase.SetRemoveAsync(RedisKeys.DeActivatedConsumers(), consumer.Id);

                var response = new ConsumerResponse(
                    id,
                    consumer.User.Email,
                    consumer.User.FirstName,
                    consumer.User.LastName,
                    consumer.User.UserName,
                    consumer.ProjectName,
                    consumer.IsActive,
                    consumer.CreatedOnUtc.ToFormatedDateTime());

                logger.LogInformation("Consumer updated successfully with ID {Id}.", id);

                return new ResponseWrapper<ConsumerResponse>(response);
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while updating consumer with ID {Id}: {ErrorMessage}", id,
                    ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Consumer not found while updating with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating consumer with ID {Id}. Request: {Request}", id,
                    JsonConvert.SerializeObject(request));
                throw ApplicationBadRequestException.Create("An unexpected error occurred while updating the consumer.",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task UpdateClientIdLifeTime(CreateConsumerModel request, ConfigurationDbContext configurationDb, string encryptionKey, Consumer consumer)
        {
            var userClientId = await GetUserClientId(configurationDb, encryptionKey, consumer);

            if (userClientId.ClientId.AccessTokenLifetime != request.TokenTtl)
            {
                userClientId.ClientId.AccessTokenLifetime = request.TokenTtl;

                configurationDb.Clients.Update(userClientId.ClientId);
                await configurationDb.SaveChangesAsync(default);
            }
        }

        private static async Task<(Client ClientId, string EncryptedClientId)> GetUserClientId(ConfigurationDbContext configurationDb, string encryptionKey, Consumer consumer)
        {
            var rowClientId = ClientIdHelper.CreateRowClientId(consumer.User.UserName, consumer.ProjectName,
            consumer.User.Id, consumer.Id);
            var encryptedClientId = ClientIdHelper.EncryptClientId(rowClientId, encryptionKey);
            var userClientId = await configurationDb.Clients.Where(item => item.ClientId == encryptedClientId).FirstOrDefaultAsync(default);
            return (userClientId, encryptedClientId);
        }

        public static async Task<bool> DeactivateAsync(
            [FromRoute] int id,
            IConsumerCommandRepository commandRepository,
            IConsumerQueryRepository queryRepository,
            IRedisDatabase redisDatabase,
            ILogger<Consumer> logger)
        {
            try
            {
                logger.LogInformation("Attempting to deactivate consumer with ID: {Id}", id);

                if (id <= 0)
                {
                    logger.LogWarning("Invalid consumer ID provided for deactivation: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                var consumer = await queryRepository.GetByIdAsync(id);
                if (consumer == null)
                {
                    logger.LogWarning("Consumer with ID {Id} not found for deactivation.", id);
                    throw ApplicationNotFoundException.Create($"Consumer with ID: {id} not found.");
                }

                consumer.Deactivate(1);

                var result = await commandRepository.UpdateAsync(consumer, default, true);

                if (result)
                {
                    logger.LogInformation("Consumer deactivated successfully with ID {Id}.", id);

                    await redisDatabase.SetAddAsync(RedisKeys.DeActivatedConsumers(), consumer.Id);
                }
                else
                {
                    logger.LogWarning("Failed to deactivate consumer with ID {Id}. Update operation returned false.",
                        id);
                    throw ApplicationBadRequestException.Create(
                        "Failed to deactivate consumer. The update operation was not successful.");
                }

                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while deactivating consumer with ID {Id}: {ErrorMessage}", id,
                    ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Consumer not found while deactivating with ID {Id}: {ErrorMessage}", id,
                    ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while deactivating consumer with ID {Id}.", id);
                throw ApplicationBadRequestException.Create(
                    "An unexpected error occurred while deactivating the consumer.",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<bool> ActivateAsync(
            [FromRoute] int id,
            bool activate,
            IConsumerCommandRepository commandRepository,
            IConsumerQueryRepository queryRepository,
            IRedisDatabase redisDatabase,
            ILogger<Consumer> logger)
        {
            try
            {
                logger.LogInformation("Attempting to Activate consumer with ID: {Id}", id);

                if (id <= 0)
                {
                    logger.LogWarning("Invalid consumer ID provided for deactivation: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                var consumer = await queryRepository.GetByIdAsync(id);
                if (consumer == null)
                {
                    logger.LogWarning("Consumer with ID {Id} not found for deactivation.", id);
                    throw ApplicationNotFoundException.Create($"Consumer with ID: {id} not found.");
                }

                if (activate)
                    consumer.Activate(1);
                else
                    consumer.Deactivate(1);

                var result = await commandRepository.UpdateAsync(consumer, default, true);

                if (result)
                {
                    logger.LogInformation("Consumer deactivated successfully with ID {Id}.", id);

                    if(activate)
                        await redisDatabase.SetRemoveAsync(RedisKeys.DeActivatedConsumers(), consumer.Id);
                    else
                        await redisDatabase.SetAddAsync(RedisKeys.DeActivatedConsumers(), consumer.Id);
                }
                else
                {
                    logger.LogWarning("Failed to Activate consumer with ID {Id}. Update operation returned false.",
                        id);
                    throw ApplicationBadRequestException.Create(
                        "Failed to Activate consumer. The update operation was not successful.");
                }

                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while Activating consumer with ID {Id}: {ErrorMessage}", id,
                    ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Consumer not found while Activating with ID {Id}: {ErrorMessage}", id,
                    ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while Activating consumer with ID {Id}.", id);
                throw ApplicationBadRequestException.Create(
                    "An unexpected error occurred while Activating the consumer.",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        private static void ValidateCreateRequest(this CreateConsumerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProjectName))
                throw ApplicationBadRequestException.Create("Project Name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw ApplicationBadRequestException.Create("Email is required.");
            if (string.IsNullOrWhiteSpace(request.UserName))
                throw ApplicationBadRequestException.Create("User Name is required.");
        }

        public static byte[] SetApiKeyCredentials(this Credentials credentials, string encryptionKey)
        {
            var stringCredentials = JsonConvert.SerializeObject(credentials);
            return CryptoHelper.Encrypt(stringCredentials, encryptionKey);
        }

        public static Credentials GetApiKeyCredentials(this byte[] credentials, string encryptionKey)
        {
            var decryptedCredentials = CryptoHelper.Decrypt(credentials, encryptionKey);
            return JsonConvert.DeserializeObject<Credentials>(decryptedCredentials);
        }

        public static async Task<ResponseWrapper<CreateConsumerModel>> GetConsumerDataForEdit(int consumerId,
            IConsumerQueryRepository consumerQueryRepository,
            IMarketTradingPairQueryRepository tradingPairQueryRepository,
            IProviderQueryRepository providerQuery,
            IMarketQueryRepository marketQuery,
            ILogger<Consumer> logger,
            string encryptionKey,
            ConfigurationDbContext configurationDb,
            CancellationToken cancellationToken)
        {
            var consumer = await consumerQueryRepository.GetConfigurationByConsumerIdWithAllIncludesAsync(consumerId, cancellationToken);

            if (consumer == null)
            {
                logger.LogError("Consumer with ID {Id} not found.", consumerId);
                return new ResponseWrapper<CreateConsumerModel>([$"Consumer with ID {consumerId} was not found."]);
            }

            try
            {
                var result = await ConsumerConfigurationHandlers.GetByIdAsync(consumerId, consumerQueryRepository, logger);

                List<string> selectedProviderIds = [.. result.ExchangeProviders.Select(item => item.Id.ToString())];
                List<string> selectedMarketIds = [.. result.Markets.Select(item => item.Id.ToString())];
                List<string> selectedPairIds = [.. result.TradingPairs.Select(item => item.Id.ToString())];


                var userClientId = await GetUserClientId(configurationDb, encryptionKey, consumer);

                CreateConsumerModel createConsumerModel = new()
                {
                    Email = result.User.Email,
                    ConsumerId = consumerId,
                    ProjectName = result.ProjectName,
                    UserName = consumer.User.UserName,
                    IsActive = result.IsActive,
                    ApiKey = result.ApiKey,
                    ProviderIds = selectedProviderIds,
                    ProviderOptions = await GetProviders(providerQuery, selectedProviderIds, isListFilter: false, cancellationToken: cancellationToken),
                    MarketIds = selectedMarketIds,
                    MarketOptions = await GetMarkets(marketQuery, selectedMarketIds, isListFilter: false, cancellationToken: cancellationToken),
                    PairOptions = await GetPairs(tradingPairQueryRepository, selectedPairIds, isListFilter: false, cancellationToken: cancellationToken),
                    PairIds = selectedPairIds,
                    ConsumerOptions = await GetConsumers(consumerQueryRepository, [result.ConsumerId.ToString()], isListFilter: false, cancellationToken: cancellationToken),
                    SpreadOptions = result.SpreadOptions,
                    ClientId = userClientId.EncryptedClientId,
                    WhiteListIps = consumer.GetWhiteListIps(),
                    TokenTtl = userClientId.ClientId?.AccessTokenLifetime ?? 86400
                };

                return new ResponseWrapper<CreateConsumerModel>(createConsumerModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bad request while getting consumer data for edit.");
                return new ResponseWrapper<CreateConsumerModel>([
                    $"Bad request while getting consumer data for edit. \\n {ex.Message}"
                ]);
            }
        }


        private static async Task<IEnumerable<SelectListItem>> GetProviders(
            IProviderQueryRepository providerQuery,
            List<string> selectedIds = null,
            List<string> selectedMarketIds = null,
            List<string> selectedPairIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];
            selectedMarketIds ??= [];
            selectedPairIds ??= [];

            var selectedMarketIdInts =
                selectedMarketIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();
            var selectedPairIdInts = selectedPairIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();

            var providers = await providerQuery.GetAllPublishedProvidersAsync(cancellationToken);

            if (selectedMarketIdInts.Count > 0 || selectedPairIdInts.Count > 0)
            {
                providers =
                [
                    .. providers
                        .Where(p =>
                            (selectedMarketIdInts.Count == 0 ||
                             p.MarketExchangeRateProviders.Any(mp => selectedMarketIdInts.Contains(mp.MarketId)))
                            &&
                            (selectedPairIdInts.Count == 0 ||
                             p.MarketTradingPairProviders.Any(pp =>
                                 selectedPairIdInts.Contains(pp.MarektTradingPairId)))
                        )
                ];
            }

            var result = new List<SelectListItem>();
            if (isListFilter)
                result.Add(new SelectListItem { Value = "", Text = "All" });

            result.AddRange(providers.Select(provider => new SelectListItem
            {
                Value = provider.Id.ToString(),
                Text = provider.Name.ToString(),
                Selected = selectedIds.Contains(provider.Id.ToString())
            }));

            return result;
        }

        private static async Task<IEnumerable<SelectListItem>> GetMarkets(
            IMarketQueryRepository marketQuery,
            List<string> selectedIds = null,
            List<string> selectedProviderIds = null,
            List<string> selectedPairIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];
            selectedProviderIds ??= [];
            selectedPairIds ??= [];

            var selectedProviderIdInts =
                selectedProviderIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();
            var selectedPairIdInts = selectedPairIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();

            var markets = await marketQuery.GetAllPublishedMarketsWithIncludesAsync(cancellationToken);

            if (selectedProviderIdInts.Count != 0)
            {
                markets =
                [
                    .. markets.Where(m =>
                        m.MarketExchangeRateProviders.Any(
                            p => selectedProviderIdInts.Contains(p.ExchangeRateProviderId)))
                ];
            }

            if (selectedPairIdInts.Count != 0)
            {
                markets = [.. markets.Where(m => m.TradingPairs.Any(p => selectedPairIdInts.Contains(p.Id)))];
            }

            var result = new List<SelectListItem>();
            if (isListFilter)
                result.Add(new SelectListItem { Value = "", Text = "All" });

            result.AddRange(markets.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.BaseCurrency.GetCurrencyName(),
                Selected = selectedIds.Contains(m.Id.ToString())
            }));

            return result;
        }

        private static async Task<IEnumerable<SelectListItem>> GetPairs(
            IMarketTradingPairQueryRepository tradingPairQueryRepository,
            List<string> selectedIds = null,
            List<string> selectedMarketIds = null,
            List<string> selectedProviderIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];
            selectedMarketIds ??= [];
            selectedProviderIds ??= [];

            var selectedMarketIdInts =
                selectedMarketIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();
            var selectedProviderIdInts =
                selectedProviderIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();

            var pairs =
                await tradingPairQueryRepository.GetAllPublishedMarketTradingPairsWithIncludesAsync(cancellationToken);

            if (selectedMarketIdInts.Count != 0)
                pairs = [.. pairs.Where(p => selectedMarketIdInts.Contains(p.MarketId))];

            if (selectedProviderIdInts.Count != 0)
                pairs =
                [
                    .. pairs.Where(p =>
                        p.Market.MarketExchangeRateProviders.Any(e =>
                            selectedProviderIdInts.Contains(e.ExchangeRateProviderId)))
                ];

            var result = new List<SelectListItem>();
            if (isListFilter)
                result.Add(new SelectListItem { Value = "", Text = "All" });

            result.AddRange(pairs.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.GetPairFormated(),
                Selected = selectedIds.Contains(p.Id.ToString())
            }));

            return result;
        }

        private static async Task<IEnumerable<SelectListItem>> GetConsumers(
            IConsumerQueryRepository consumerQueryRepository,
            List<string> selectedIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];

            var consumers = await consumerQueryRepository.GetAllWithAllIncludesAsync(cancellationToken);

            var result = new List<SelectListItem>();
            if (isListFilter)
                result.Add(new SelectListItem { Value = "", Text = "All" });

            result.AddRange(consumers.Select(Consumer => new SelectListItem
            {
                Value = Consumer.Id.ToString(),
                Text = Consumer.User.UserName,
                Selected = selectedIds.Contains(Consumer.Id.ToString())
            }));

            return result;
        }

        public static async Task<(string clientId, string clientSecret)> GenerateClientSecret(int consumerId,
            IConsumerQueryRepository consumerQueryRepository,
            IApiKeyClientService apiKeyClientService,
            string encryptionKey,
            CancellationToken cancellationToken)
        {
            var consumer = await consumerQueryRepository.GetByIdWithAllIncludesAsync(consumerId, cancellationToken);

            var apiKey = await apiKeyClientService.GenerateClientSecretAsync(consumer.User.UserName,
                consumer.ProjectName,
                consumer.User.Id, consumerId, encryptionKey, cancellationToken);

            return (apiKey.ClientId, apiKey.ClientSecret);
        }

        public static GetConsumerInformationModel GetConsumerInformationAsync(UserManager<User> userManager)
        {
            var users = userManager.Users.Where(u => !u.IsDeleted);

            return new GetConsumerInformationModel
            {
                Users = [.. users.Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName
                })]
            };
        }

        public static async Task<ResponseWrapper<int>> AddConsumerInformation(CreateConsumerModel request,
            IConsumerCommandRepository consumerCommandRepository, IConsumerQueryRepository consumerQueryRepository,
            UserManager<User> userManager, ILogger<Consumer> logger, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ProjectName))
            {
                logger.LogError("Project Name is required.");
                return new ResponseWrapper<int>(["Project Name is required."]);

            }

            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                logger.LogError("User with ID {UserId} not found.", request.UserId);
                return new ResponseWrapper<int>(["User does not exist."]);
            }

            var consumer = new Consumer(user.Id, request.ProjectName, user.Id);
            await consumerCommandRepository.AddAsync(consumer, cancellationToken);
            await consumerCommandRepository.SaveChangesAsync(cancellationToken);

            return new ResponseWrapper<int>(consumer.Id);
        }
    }
}