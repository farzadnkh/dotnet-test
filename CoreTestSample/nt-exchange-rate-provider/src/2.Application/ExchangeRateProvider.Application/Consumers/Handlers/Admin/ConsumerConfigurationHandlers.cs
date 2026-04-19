using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Dtos.Responses;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Application.Consumers.Handlers.Admin;

public static class ConsumerConfigurationHandlers
{
    public static async Task<IPaginationResponse<ConsumerConfigurationResponse>> GetAllWithPaginationAsync(
        [FromQuery] int? index,
        [FromQuery] RequestPagingSize? size,
        [AsParameters] ConsumerConfigurationPaginatedFilterRequest consumerPaginatedFilterRequest,
        IConsumerQueryRepository repo,
        ILogger<Consumer> logger)
    {
        try
        {
            logger.LogInformation("Attempting to retrieve all consumerConfigurations with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                index, size, JsonConvert.SerializeObject(consumerPaginatedFilterRequest));

            PaginationRequest<ConsumerConfigurationPaginatedFilterRequest> request = new()
            {
                Filter = consumerPaginatedFilterRequest,
                Paging = new()
                {
                    Index = index ?? 0,
                    Size = size ?? 0
                }
            };

            var result = await repo.GetConsumersConfigurationWithPaginationAndFilterAsync(request, default);
            logger.LogInformation("Successfully retrieved {Count} consumerConfigurations.", result.Base.Total);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error occurred while retrieving consumerConfigurations with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                index, size, JsonConvert.SerializeObject(consumerPaginatedFilterRequest));
            throw ApplicationBadRequestException.Create("Error occurred while retrieving consumerConfigurations.", (int)HttpStatusCode.InternalServerError);
        }
    }

    public static async Task<ConsumerConfigurationResponse> GetByIdAsync(
        [FromRoute] int consumerId,
        IConsumerQueryRepository repo,
        ILogger<Consumer> logger)
    {
        try
        {
            logger.LogInformation("Attempting to retrieve consumerConfiguration with ConsumerID: {Id}", consumerId);

            if (consumerId <= 0)
            {
                logger.LogWarning("Invalid consumerConfiguration ConsumerId provided: {Id}", consumerId);
                throw ApplicationBadRequestException.Create("ConsumerId is required and must be a positive integer.");
            }

            var configs = await repo.GetConfigurationByConsumerIdWithAllIncludesAsync(consumerId);

            if (configs == null)
            {
                logger.LogWarning("ConsumerConfiguration with ConsumerId {Id} not found.", consumerId);
                throw ApplicationNotFoundException.Create($"Consumer with ID: {consumerId} not found.");
            }

            logger.LogInformation("Consumer with ID {Id} successfully retrieved.", consumerId);
            return new()
            {
                Id = configs.Id,
                ExchangeProviders = [.. configs.ConsumerProviders.Select(item => item.Provider)],
                Markets = [.. configs.ConsumerMarkets.Select(item => item.Market)],
                TradingPairs = [.. configs.ConsumerPairs.Select(item => item.MarketTradingPair)],
                ConsumerId = configs.Id,
                IsActive = configs.IsActive,
                SpreadOptions = repo.GetSpreadOptions(configs.ConsumerPairs, configs.ConsumerMarkets),
                ProjectName = configs.ProjectName,
                User = configs.User,
                ConsumerUsername = configs.User.UserName,
                ApiKey = configs.Apikey
            };
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Bad request for consumerConfiguration with consumerId {Id}: {ErrorMessage}", consumerId, ex.Message);
            throw;
        }
        catch (ApplicationNotFoundException ex)
        {
            logger.LogWarning(ex, "consumerConfiguration not found with consumerId {Id}: {ErrorMessage}", consumerId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving consumerConfiguration with consumerId {Id}.", consumerId);
            throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving the consumerConfiguration.", (int)HttpStatusCode.InternalServerError);
        }
    }

    public static async Task<ConsumerConfigurationResponse> CreateAsync(
        [FromBody] CreateConsumerConfigurationRequest request,
        IConsumerCommandRepository repo,
        IConsumerQueryRepository queryRepository,
        IMarketTradingPairQueryRepository marketTradingPairQuery,
        IMarketQueryRepository marketQueryRepository,
        ILogger<Consumer> logger)
    {
        logger.LogInformation("Creating consumer configuration. ConsumerId: {ConsumerId}", request.ConsumerId);

        try
        {
            request.ValidateCreateRequest();

            var consumer = await queryRepository.GetConfigurationByConsumerIdWithAllIncludesAsync(request.ConsumerId);
            if (consumer is null)
            {
                const string notFoundMsg = "Consumer not found";
                logger.LogWarning("{Message}. Id: {ConsumerId}", notFoundMsg, request.ConsumerId);
                throw ApplicationNotFoundException.Create($"{notFoundMsg}. Id: {request.ConsumerId}");
            }

            if (request.TradingPairIds?.Any() == true)
            {
                var tradingPairs = await LoadTradingPairsAsync(request.TradingPairIds, marketTradingPairQuery);
                foreach (var pair in tradingPairs)
                {
                    consumer.AddTradingPairs(pair.Id, pair.MarketId, request.CreatedById, request.IsActive, request.SpreadOptions);
                    consumer.AddMarket(pair.MarketId, request.CreatedById, request.IsActive, null);

                    foreach (var provider in pair.MarketTradingPairProviders.Select(p => p.ExchangeRateProvider))
                    {
                        consumer.AddProvider(provider.Id, request.IsActive, request.CreatedById);
                    }
                }

                await UpdateConsumerAsync(request.CreatedById, consumer, repo, logger);
                return CreateResponse(consumer, request, queryRepository);
            }

            if (request.MarketIds?.Any() == true)
            {
                var markets = await LoadMarketsAsync(request.MarketIds, marketQueryRepository);
                foreach (var market in markets)
                {
                    consumer.AddMarket(market.Id, request.CreatedById, request.IsActive, request.SpreadOptions);

                    foreach (var provider in market.MarketExchangeRateProviders.Select(p => p.ExchangeRateProvider))
                    {
                        consumer.AddProvider(provider.Id, request.IsActive, request.CreatedById);
                    }
                }

                await UpdateConsumerAsync(request.CreatedById, consumer, repo, logger);
                return CreateResponse(consumer, request, queryRepository);
            }

            if (request.ExchangeRateProviderIds?.Any() == true)
            {
                foreach (var providerId in request.ExchangeRateProviderIds.Distinct())
                {
                    consumer.AddProvider(Convert.ToInt32(providerId), request.IsActive, request.CreatedById);
                }

                await UpdateConsumerAsync(request.CreatedById, consumer, repo, logger);
                return CreateResponse(consumer, request, queryRepository);
            }

            logger.LogWarning("No valid configuration options found for ConsumerId: {ConsumerId}", request.ConsumerId);
            throw ApplicationBadRequestException.Create("No valid configuration data provided.");
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating configuration for ConsumerId: {ConsumerId}. Request: {Request}",
                request.ConsumerId, JsonConvert.SerializeObject(request));
            throw ApplicationBadRequestException.Create("Unexpected error while creating configuration.", (int)HttpStatusCode.InternalServerError);
        }
    }

    public static async Task<ConsumerConfigurationResponse> UpdateAsync(
        [FromBody] CreateConsumerConfigurationRequest request,
        IConsumerCommandRepository repo,
        IConsumerQueryRepository queryRepository,
        IMarketTradingPairQueryRepository marketTradingPairQuery,
        IMarketQueryRepository marketQueryRepository,
        INotifier notifier,
        ILogger<Consumer> logger)
    {
        try
        {
            logger.LogInformation("Updating configuration for ConsumerId: {ConsumerId}", request.ConsumerId);

            request.ValidateCreateRequest();

            var consumer = await queryRepository.GetConfigurationByConsumerIdWithAllIncludesAsync(request.ConsumerId);
            if (consumer is null)
            {
                logger.LogWarning("Consumer with ID {ConsumerId} not found.", request.ConsumerId);
                throw ApplicationNotFoundException.Create($"Consumer with ID {request.ConsumerId} not found.");
            }

            await repo.DeleteAllConfigurations(consumer.ConsumerMarkets, consumer.ConsumerPairs, consumer.ConsumerProviders);
            consumer.ClearAssociations();

            if (request.TradingPairIds?.Any() == true)
            {
                var tradingPairs = await LoadTradingPairsAsync(request.TradingPairIds, marketTradingPairQuery);
                foreach (var pair in tradingPairs)
                    consumer.AddTradingPairs(pair.Id, pair.MarketId, request.CreatedById, request.IsActive, request.SpreadOptions);
            }

            if (request.MarketIds?.Any() == true)
            {
                var markets = await LoadMarketsAsync(request.MarketIds, marketQueryRepository);
                foreach (var market in markets)
                    consumer.AddMarket(market.Id, request.CreatedById, request.IsActive, request.SpreadOptions);
            }

            if (request.ExchangeRateProviderIds?.Any() == true)
            {
                foreach (var providerId in request.ExchangeRateProviderIds.Distinct())
                    consumer.AddProvider(Convert.ToInt32(providerId), request.IsActive, request.CreatedById);
            }

            await UpdateConsumerAsync(request.CreatedById, consumer, repo, logger);

            await notifier.SyncClientSideSocketAsync(new()
            {
                ConsumerId = consumer.Id,
                MessageType = MessageType.ConsumerPairChanged
            });

            logger.LogInformation("No configuration changes were made for ConsumerId: {ConsumerId}", request.ConsumerId);
            return CreateResponse(consumer, request, queryRepository);
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Validation failed during update: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during consumer configuration update. Request: {Request}", JsonConvert.SerializeObject(request));
            throw ApplicationBadRequestException.Create("An unexpected error occurred while updating the consumer.", (int)HttpStatusCode.InternalServerError);
        }
    }

    #region Utilities

    private static void ValidateCreateRequest(this CreateConsumerConfigurationRequest request)
    {
        if (request.ConsumerId <= 0)
            throw ApplicationBadRequestException.Create("ConsumerId is required.");
    }

    private static async Task<List<MarketTradingPair>> LoadTradingPairsAsync(
        IEnumerable<string> ids,
        IMarketTradingPairQueryRepository query)
    {
        var results = new List<MarketTradingPair>();

        foreach (var id in ids.Distinct())
        {
            var item = await query.GetByIdWithAllIncludesAsync(Convert.ToInt32(id));
            if (item != null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    private static async Task<List<Market>> LoadMarketsAsync(
        IEnumerable<string> ids,
        IMarketQueryRepository query)
    {
        var distinctIntIds = ids?.Select(id => {
            if (int.TryParse(id, out int intId))
                return (int?)intId;
            return null;
        }).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

        if (distinctIntIds == null || distinctIntIds.Count == 0)
        {
            return [];
        }

        var result = await query.GetByIdWithAllIncludesAsync(distinctIntIds);
        return result;
    }

    private static async Task UpdateConsumerAsync(int userId, Consumer consumer, IConsumerCommandRepository repo, ILogger logger)
    {
        try
        {
            consumer.ConsumerMarkets = [.. consumer.ConsumerMarkets.DistinctBy(x => x.MarketId)];
            consumer.ConsumerPairs = [.. consumer.ConsumerPairs.DistinctBy(x => x.PairId)];
            consumer.ConsumerProviders = [.. consumer.ConsumerProviders.DistinctBy(x => x.ProviderId)];

            await repo.UpdateAsync(consumer, userId, default, withSaveChanged: true);
            logger.LogInformation("Consumer configuration Updated. ConsumerId: {ConsumerId}", consumer.Id);
        }
        catch (Exception ex)
        {
            logger.LogCritical("Error");
            throw ApplicationBadRequestException.Create(ex.ToString());
        }
    }

    private static ConsumerConfigurationResponse CreateResponse(
        Consumer consumer,
        CreateConsumerConfigurationRequest request,
        IConsumerQueryRepository query)
    {
        try
        {
            //return new ConsumerConfigurationResponse
            //{
            //    Id = consumer.Id,
            //    ConsumerId = consumer.Id,
            //    IsActive = request.IsActive,
            //    ExchangeProviders = [.. consumer.ConsumerProviders.Select(p => p.Provider)],
            //    Markets = [.. consumer.ConsumerMarkets.Select(m => m.Market)],
            //    TradingPairs = [.. consumer.ConsumerPairs.Select(tp => tp.MarketTradingPair)],
            //    SpreadOptions = query.GetSpreadOptions(consumer.ConsumerPairs, consumer.ConsumerMarkets)
            //};

            var s = new ConsumerConfigurationResponse();
            s.Id = consumer.Id;
            s.ConsumerId = consumer.Id;
            s.IsActive = request.IsActive;
            s.ExchangeProviders = [.. consumer.ConsumerProviders.Select(p => p.Provider)];
            s.Markets = [.. consumer.ConsumerMarkets.Select(m => m.Market)];
            s.TradingPairs = [.. consumer.ConsumerPairs.Select(tp => tp.MarketTradingPair)];
            s.SpreadOptions = query.GetSpreadOptions(consumer.ConsumerPairs, consumer.ConsumerMarkets);

            return s;
        }
        catch (Exception ex)
        {

            throw;
        }
    }
    #endregion
}