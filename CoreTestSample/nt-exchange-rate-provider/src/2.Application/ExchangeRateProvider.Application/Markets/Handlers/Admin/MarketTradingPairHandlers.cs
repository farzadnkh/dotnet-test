using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using Microsoft.Extensions.Logging;

namespace ExchangeRateProvider.Application.Markets.Handlers.Admin
{
    public static class MarketTradingPairHandlers
    {
        public static async Task<IPaginationResponse<MarketTradingPairResponse>> GetAllWithPaginationAsync(
            [FromQuery] int? index,
            [FromQuery] RequestPagingSize? size,
            [AsParameters] MarketTradingPairPaginatedFilterRequest marketPaginatedFilterRequest,
            IMarketTradingPairQueryRepository repo,
            ILogger<MarketTradingPair> logger)
        {
            logger.LogInformation(
                "Attempting to retrieve MarketTradingPairs with pagination. Index: {Index}, Size: {Size}, Filter: {@MarketPaginatedFilterRequest}",
                index, size, marketPaginatedFilterRequest);

            try
            {
                PaginationRequest<MarketTradingPairPaginatedFilterRequest> request = new()
                {
                    Filter = marketPaginatedFilterRequest,
                    Paging = new()
                    {
                        Index = index ?? 0,
                        Size = size ?? RequestPagingSize.Five
                    }
                };

                var result = await repo.GetMarketTradingPairsWithPaginationAndFilterAsync(request, default);

                logger.LogInformation("Successfully retrieved {Count} MarketTradingPairs. Total items: {TotalCount}", result.Items.Count, result.Base.Total);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving MarketTradingPairs. Request Index: {Index}, Size: {Size}, Filter: {@MarketPaginatedFilterRequest}",
                                index, size, marketPaginatedFilterRequest);

                throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving market trading pairs. Please try again later or contact support if the issue persists.");
            }
        }

        public static async Task<MarketTradingPairResponse> GetByIdAsync(
            [FromRoute] int id,
            IMarketTradingPairQueryRepository repo,
            ILogger<MarketTradingPair> logger)
        {
            logger.LogInformation("Attempting to retrieve MarketTradingPair with ID: {Id}", id);

            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Invalid ID provided for GetByIdAsync: {Id}", id);
                    throw ApplicationBadRequestException.Create("A valid MarketTradingPair ID is required.");
                }

                MarketTradingPair marketTradingPair = await repo.GetByIdWithAllIncludesAsync(id)
                    ?? throw ApplicationNotFoundException.Create($"MarketTradingPair with ID: {id} not found.");

                var result = new MarketTradingPairResponse(
                    marketTradingPair.Id,
                    $"{marketTradingPair.Currency.Name} ({marketTradingPair.Currency.Code})",
                    $"{marketTradingPair.Currency.Code}/{marketTradingPair.Market.BaseCurrency.Code}",
                    marketTradingPair.Published,
                    marketTradingPair.SpreadOptions,
                    marketTradingPair.Description,
                    currencyEntity: marketTradingPair.Currency,
                    market: marketTradingPair.Market,
                    exchangeProviders: [.. marketTradingPair.MarketTradingPairProviders?.Select(item => item.ExchangeRateProvider)],
                    ratingMethod: marketTradingPair.RatingMethod);

                logger.LogInformation("MarketTradingPair with ID {Id} successfully retrieved.", id);
                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request for MarketTradingPair ID {Id}.", id);
                throw ApplicationBadRequestException.Create(string.IsNullOrWhiteSpace(ex.Message) ? $"Bad request when trying to retrieve MarketTradingPair with ID {id}." : ex.Message, ex);
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "MarketTradingPair not found with ID {Id}.", id);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving MarketTradingPair with ID {Id}.", id);
                throw ApplicationBadRequestException.Create($"An unexpected error occurred while retrieving MarketTradingPair with ID {id}.", ex);
            }
        }

        public static async Task<MarketTradingPairResponse> CreateAsync(
            [FromBody] CreateMarketTradingPairRequest request,
            IMarketTradingPairCommandRepository commandRepo,
            ICurrencyQueryRepository currencyQueryRepository,
            IMarketQueryRepository marketQueryRepository,
            IMarketTradingPairQueryRepository queryRepository,
            ILogger<MarketTradingPair> logger)
        {
            logger.LogInformation("Attempting to create MarketTradingPair with request: {@CreateMarketTradingPairRequest}", request);

            try
            {
                (Currency currency, Market market) = await request.ValidateRequest(marketQueryRepository, currencyQueryRepository, queryRepository);

                var marketTradingPair = MarketTradingPair.Create(
                    request.MarketId,
                    request.CurrencyId,
                    request.CreatedById,
                    request.Published,
                    request.Description);

                await commandRepo.AddAsync(marketTradingPair);

                var savedChangesResult = await commandRepo.SaveChangesAsync(request.CreatedById, default);
                var newId = marketTradingPair.Id;

                var result = new MarketTradingPairResponse(
                    newId,
                    $"{currency.Name} ({currency.Code})",
                    $"{currency.Code}/{market.BaseCurrency.Code}",
                    marketTradingPair.Published,
                    marketTradingPair.SpreadOptions,
                    marketTradingPair.Description,
                    market,
                    currency);

                logger.LogInformation("MarketTradingPair created successfully with ID {Id}.", newId);
                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while creating MarketTradingPair. Request: {@CreateMarketTradingPairRequest}", request);
                throw ApplicationBadRequestException.Create(string.IsNullOrWhiteSpace(ex.Message) ? "Bad request during MarketTradingPair creation." : ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating MarketTradingPair. Request: {@CreateMarketTradingPairRequest}", request);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while creating the MarketTradingPair.", ex); // Wrap inner exception
            }
        }

        public static async Task UpdateAsync(
            [FromBody] CreateMarketTradingPairRequest request,
            [FromRoute] int id,
            IMarketTradingPairCommandRepository commandRepository,
            IMarketTradingPairQueryRepository queryRepository,
            IProviderQueryRepository providerRepository,
            INotifier notifier,
            IMarketQueryRepository marketQueryRepository,
            ICurrencyQueryRepository currencyQueryRepository,
            ILogger<MarketTradingPair> logger)
        {
            logger.LogInformation("Attempting to update MarketTradingPair with ID: {Id}. Request: {@UpdateMarketTradingPairRequest}", id, request);

            try
            {
                var marketTradingPair =  await request.ValidateRequest(id, marketQueryRepository, currencyQueryRepository, queryRepository);

                var ids = JsonConvert.DeserializeObject<List<string>>(request.ExchangeRateProviderIds[0]);
                var providerIds = ids?
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .Distinct()
                    .ToList() ?? [];

                marketTradingPair.Update(
                    request.MarketId,
                    request.CurrencyId,
                    request.SpreadOptions,
                    request.Description,
                    request.Published,
                    request.RatingMethod);

                var providerTypes = new List<ProviderType>();
                foreach (var providerId in providerIds)
                {
                    var provider = await providerRepository.GetByIdAsync(providerId);
                    if (provider == null)
                    {
                        logger.LogWarning("Exchange rate provider with ID {ProviderId} not found for market update. Cannot associate with market.", providerId);
                        continue;
                    }

                    providerTypes.Add(provider.Type);
                    marketTradingPair.SetExchangeRateProviders(providerIds);
                }

                await commandRepository.UpdateAsync(marketTradingPair, default, true);

                foreach (var type in providerTypes.Distinct())
                {
                    await notifier.SyncBackgroundJobsAsync(new BackgroundJobSyncMessageArgs
                    {
                        ProviderType = type
                    });
                }

                await notifier.SyncClientSideSocketAsync(new()
                {
                    MessageType = MessageType.ConsumerPairChanged
                });

                logger.LogInformation("MarketTradingPair updated successfully with ID {Id}.", id);
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while updating MarketTradingPair with ID {Id}. Request: {@UpdateMarketTradingPairRequest}", id, request);
                throw ApplicationBadRequestException.Create(string.IsNullOrWhiteSpace(ex.Message) ? $"Bad request when updating MarketTradingPair with ID {id}." : ex.Message, ex);
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "MarketTradingPair not found with ID {Id} during update attempt.", id);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating MarketTradingPair with ID {Id}. Request: {@UpdateMarketTradingPairRequest}", id, request);
                throw ApplicationBadRequestException.Create($"An unexpected error occurred while updating MarketTradingPair with ID {id}.", ex);
            }
        }

        private static async Task<(Currency, Market)> ValidateRequest(this CreateMarketTradingPairRequest request,
            IMarketQueryRepository marketQueryRepository,
            ICurrencyQueryRepository currencyQueryRepository,
            IMarketTradingPairQueryRepository queryRepository)
        {
            if (request.CurrencyId <= 0)
                throw ApplicationBadRequestException.Create("CurrencyId is required and must be a valid ID.");

            if (request.MarketId <= 0)
                throw ApplicationBadRequestException.Create("MarketId is required and must be a valid ID.");

            var currency = await currencyQueryRepository.GetByIdAsync(request.CurrencyId)
                ?? throw ApplicationBadRequestException.Create($"Currency not found. Cannot create MarketTradingPair.");

            var market = await marketQueryRepository.GetByIdWithAllIncludesAsync(request.MarketId)
                 ?? throw ApplicationBadRequestException.Create($"Market not found. Cannot create MarketTradingPair.");

            var existingPair = await queryRepository.GetByCurrencyCodeAndMarketAsync(currency.Code, request.MarketId);
            if (existingPair is not null)
                throw ApplicationBadRequestException.Create($"Market TradingPair with CurrencyCode: {currency.Code} and Market: {market.BaseCurrency.Code} is Already Exist.");

            if (currency.Code.Equals(market.BaseCurrency.Code))
                throw ApplicationBadRequestException.Create("Currency and Market Must Differ. Update Failed.");

            return (currency, market);
        }


        private static async Task<MarketTradingPair> ValidateRequest(this CreateMarketTradingPairRequest request,
            int id,
            IMarketQueryRepository marketQueryRepository,
            ICurrencyQueryRepository currencyQueryRepository,
            IMarketTradingPairQueryRepository queryRepository)
        {
            if (id <= 0)
                throw ApplicationBadRequestException.Create("A valid MarketTradingPair ID is required for update.");

            if (request.CurrencyId <= 0)
                throw ApplicationBadRequestException.Create("CurrencyId is required and must be a valid ID.");

            if (request.MarketId <= 0)
                throw ApplicationBadRequestException.Create("MarketId is required and must be a valid ID.");

            var currency = await currencyQueryRepository.GetByIdAsync(request.CurrencyId)
                ?? throw ApplicationBadRequestException.Create($"Currency not found. Cannot create MarketTradingPair.");

            var market = await marketQueryRepository.GetByIdWithAllIncludesAsync(request.MarketId)
                 ?? throw ApplicationBadRequestException.Create($"Market not found. Cannot create MarketTradingPair.");

            var marketTradingPair = await queryRepository.GetByIdWithAllIncludesAsync(id)
                     ?? throw ApplicationNotFoundException.Create($"MarketTradingPair with ID: {id} not found. Update failed.");

            var existingPair = await queryRepository.GetByCurrencyCodeAndMarketAsync(currency.Code, request.MarketId);
            if (existingPair is not null 
                && marketTradingPair.CurrencyId != currency.Id 
                && marketTradingPair.Market.BaseCurrencyId != market.Id)
                throw ApplicationBadRequestException.Create($"Market TradingPair with CurrencyCode: {currency.Code} and Market: {market.BaseCurrency.Code} is Already Exist.");

            if (currency.Code.Equals(market.BaseCurrency.Code))
                throw ApplicationBadRequestException.Create("Currency and Market Must Differ. Update Failed.");

            return marketTradingPair;
        }
    }
}