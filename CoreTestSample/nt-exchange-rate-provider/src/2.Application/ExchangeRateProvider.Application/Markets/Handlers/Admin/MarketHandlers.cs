using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Application.Markets.Handlers.Admin
{
    public static class MarketHandlers
    {
        public static async Task<IPaginationResponse<MarketResponse>> GetAllWithPaginationAsync(
            [FromQuery] int? index,
            [FromQuery] RequestPagingSize? size,
            [AsParameters] MarketPaginatedFilterRequest marketPaginatedFilterRequest,
            IMarketQueryRepository repo,
            ILogger<Market> logger)
        {
            try
            {
                logger.LogInformation("Attempting to retrieve all markets with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                    index, size, JsonConvert.SerializeObject(marketPaginatedFilterRequest));

                PaginationRequest<MarketPaginatedFilterRequest> request = new()
                {
                    Filter = marketPaginatedFilterRequest,
                    Paging = new()
                    {
                        Index = index ?? 0,
                        Size = size ?? 0
                    }
                };

                var result = await repo.GetMarketsWithPaginationAndFilterAsync(request, default);
                logger.LogInformation("Successfully retrieved {Count} markets.", result.Base.Total);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving markets with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                    index, size, JsonConvert.SerializeObject(marketPaginatedFilterRequest));
                throw ApplicationBadRequestException.Create("Error occurred while retrieving markets.", (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<MarketResponse> GetByIdAsync(
            [FromRoute] int id,
            IMarketQueryRepository repo,
            ILogger<Market> logger)
        {
            try
            {
                logger.LogInformation("Attempting to retrieve market with ID: {Id}", id);

                if (id <= 0) // Changed to <= 0 for robustness
                {
                    logger.LogWarning("Invalid market ID provided: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                Market market = await repo.GetByIdWithAllIncludesAsync(id);

                if (market == null)
                {
                    logger.LogWarning("Market with ID {Id} not found.", id);
                    throw ApplicationNotFoundException.Create($"Market with ID: {id} not found.");
                }

                var selectedProviders = market.MarketExchangeRateProviders?.Select(item => item.ExchangeRateProvider).ToList() ?? [];

                var result = new MarketResponse(
                    market.Id,
                    market.BaseCurrencyId,
                    market.CalculationTerm,
                    market.Published,
                    market.IsDefault,
                    market.RatingMethod,
                    market.BaseCurrency?.Name,
                    market.BaseCurrency?.Code,
                    selectedProviders,
                    market.SpreadOptions);

                logger.LogInformation("Market with ID {Id} successfully retrieved.", id);
                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request for market ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Market not found with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving market with ID {Id}.", id);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving the market.", (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<MarketResponse> CreateAsync(
            [FromBody] CreateMarketRequest request,
            IMarketCommandRepository repo,
            IMarketQueryRepository marketQueryRepository,
            IMarketTradingPairCommandRepository pairCommandRepository,
            ICurrencyQueryRepository currencyQuery,
            ILogger<Market> logger)
        {
            try
            {
                logger.LogInformation("Attempting to create a new market. Request: {Request}", JsonConvert.SerializeObject(request));

                request.ValidateCreateRequest();

                var isMarketExist = await marketQueryRepository.IsExistAsync(request.MarketCurrencyId, default);

                if (isMarketExist)
                {
                    logger.LogWarning("Market creation failed: Market with BaseCurrencyId '{MarketCurrencyId}' already exists.", request.MarketCurrencyId);
                    throw ApplicationBadRequestException.Create($"Market with BaseCurrencyId: {request.MarketCurrencyId} already exists.");
                }

                var market = Market.Create(
                    request.MarketCurrencyId,
                    request.MarketCalculationTerm ?? MarketCalculationTerm.Average,
                    false,
                    request.CreatedById,
                    request.IsDefault,
                    request.Published);

                await repo.AddAsync(market);
                var id = await repo.SaveChangesAsync(request.CreatedById, default);


                if (request.ExchangeRateProviderIds != null && request.ExchangeRateProviderIds.Count != 0)
                {
                    foreach (var providerIdStr in request.ExchangeRateProviderIds)
                    {
                        if (int.TryParse(providerIdStr, out int providerId))
                            market.AddExchangeRateProviders(providerId);
                        else
                            logger.LogWarning("Invalid exchange rate provider ID format: '{ProviderIdStr}'", providerIdStr);
                    }
                }


                await CreateTradingPairs(
                    request.CreateAllFiats,
                    request.CreateAllCryptos,
                    market,
                    pairCommandRepository,
                    currencyQuery,
                    logger);

                var result = new MarketResponse(
                    market.Id,
                    market.BaseCurrencyId,
                    market.CalculationTerm,
                    market.Published,
                    market.IsDefault,
                    RatingMethod.Automatic,
                    market.BaseCurrency?.Name,
                    market.BaseCurrency?.Code,
                    market.MarketExchangeRateProviders?.Select(m => m.ExchangeRateProvider).ToList() ?? [],
                    market.SpreadOptions);

                logger.LogInformation("Market created successfully with ID {Id}.", id);
                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while creating market: {ErrorMessage}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating market. Request: {Request}", JsonConvert.SerializeObject(request));
                throw ApplicationBadRequestException.Create("An unexpected error occurred while creating the market.", (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<MarketResponse> UpdateAsync(
            [FromBody] CreateMarketRequest request,
            [FromRoute] int id,
            IMarketCommandRepository commandRepository,
            IMarketQueryRepository queryRepository,
            IProviderQueryRepository providerRepository,
            IMarketTradingPairCommandRepository marketTradingPairCommandRepository,
            INotifier notifier,
            ILogger<Market> logger)
        {
            try
            {
                logger.LogInformation("Attempting to update market with ID: {Id}. Request: {Request}", id, JsonConvert.SerializeObject(request));

                request.ValidateCreateRequest();

                if (id <= 0)
                {
                    logger.LogWarning("Invalid market ID provided for update: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                var market = await queryRepository.GetByIdWithAllIncludesAsync(id);
                if (market == null)
                {
                    logger.LogWarning("Market with ID {Id} not found for update.", id);
                    throw ApplicationNotFoundException.Create($"Market with ID: {id} not found.");
                }
                
               var ids = JsonConvert.DeserializeObject<List<string>>(request.ExchangeRateProviderIds[0]);

                var providerIds = ids?
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .Distinct()
                    .ToList() ?? [];

                var providerTypes = new List<ProviderType>();
                market.MarketExchangeRateProviders.Clear();
                
                if (providerIds.Count > 0 && !providerIds.Equals(market.MarketExchangeRateProviders.Select(item => item.ExchangeRateProviderId)))
                {
                    foreach (var providerId in providerIds)
                    {
                        var provider = await providerRepository.GetByIdAsync(providerId);
                        if (provider == null)
                        {
                            logger.LogWarning("Exchange rate provider with ID {ProviderId} not found for market update. Cannot associate with market.", providerId);
                            continue;
                        }

                        providerTypes.Add(provider.Type);
                        market.AddExchangeRateProviders(providerId);
                    }

                    await marketTradingPairCommandRepository.BulkUpdateProviderBaseOnMarketRatingMethodAsync(market.Id, providerIds, default);
                }

                if (market.RatingMethod != request.RatingMethod)
                    await marketTradingPairCommandRepository.BulkUpdateRatingMethodBaseOnMarketRatingMethodAsync(market.Id, request.RatingMethod, default);

                market.Update(
                    request.MarketCurrencyId,
                    request.MarketCalculationTerm ?? MarketCalculationTerm.Average,
                    request.SpreadOptions,
                    request.RatingMethod,
                    request.IsDefault,
                    request.Published);

                await commandRepository.UpdateAsync(market, default, true);

                var selectedProviders = market.MarketExchangeRateProviders?
                    .Select(m => m.ExchangeRateProvider)
                    .ToList() ?? [];

                foreach (var type in providerTypes.Distinct())
                {
                    await notifier.SyncBackgroundJobsAsync(new BackgroundJobSyncMessageArgs
                    {
                        ProviderType = type
                    });
                }

                await notifier.SyncClientSideSocketAsync(new()
                {
                    MessageType = MessageType.ConsumerMarketChanged
                });

                var response = new MarketResponse(
                    market.Id,
                    market.BaseCurrencyId,
                    market.CalculationTerm,
                    market.Published,
                    market.IsDefault,
                    market.RatingMethod,
                    market.BaseCurrency?.Name,
                    market.BaseCurrency?.Code,
                    selectedProviders,
                    market.SpreadOptions);

                logger.LogInformation("Market updated successfully with ID {Id}.", id);
                return response;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while updating market with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Market not found while updating with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating market with ID {Id}. Request: {Request}", id, JsonConvert.SerializeObject(request));
                throw ApplicationBadRequestException.Create("An unexpected error occurred while updating the market.", (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task CreateTradingPairs(
            bool createAllFiats,
            bool createAllCrypto,
            Market market,
            IMarketTradingPairCommandRepository pairCommandRepository,
            ICurrencyQueryRepository currencyQuery,
            ILogger<Market> logger)
        {
            if (!createAllFiats && !createAllCrypto)
                return;

            logger.LogInformation("Creating trading pairs for market ID {MarketId}. CreateAllFiats: {CreateAllFiats}, CreateAllCrypto: {CreateAllCrypto}",
                market.Id, createAllFiats, createAllCrypto);

            var currencies = await currencyQuery.GetAllAsync();
            var pairList = new List<MarketTradingPair>();

            if (createAllFiats)
            {
                var publishedFiats = currencies
                    .Where(item => item.Published && item.Type == CurrencyType.Fiat)
                    .ToList();

                foreach (var currency in publishedFiats)
                {
                    if (currency.Id != market.BaseCurrencyId)
                    {
                        pairList.Add(MarketTradingPair.Create(market.Id, currency.Id, market.CreatedById, true, "", market.RatingMethod));
                    }
                }
            }

            if (createAllCrypto)
            {
                var publishedCryptos = currencies
                    .Where(item => item.Published && item.Type == CurrencyType.Crypto)
                    .ToList();

                foreach (var currency in publishedCryptos)
                {
                    if (currency.Id != market.BaseCurrencyId)
                    {
                        pairList.Add(MarketTradingPair.Create(market.Id, currency.Id, market.CreatedById, true, "", market.RatingMethod));
                    }
                }
            }

            if (pairList.Count != 0)
            {
                await pairCommandRepository.AddRangeAsync(pairList);
                await pairCommandRepository.SaveChangesAsync(market.CreatedById, default);
                logger.LogInformation("Successfully created {Count} trading pairs for market ID {MarketId}.", pairList.Count, market.Id);
            }
            else
            {
                logger.LogInformation("No trading pairs were generated for market ID {MarketId}.", market.Id);
            }
        }

        private static void ValidateCreateRequest(this CreateMarketRequest request)
        {
            if (request.MarketCurrencyId <= 0)
                throw ApplicationBadRequestException.Create("MarketCurrencyId is required and must be a positive integer.");
            if(request.RatingMethod == RatingMethod.None )
                throw ApplicationBadRequestException.Create("RatingMethod Should be Specify.");
        }
    }
}