using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.Currencies.Dtos.Requests;
using ExchangeRateProvider.Contract.Currencies.Dtos.Responses;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using Microsoft.Extensions.Logging;
using static MassTransit.ValidationResultExtensions;

namespace ExchangeRateProvider.Application.Currencies.Handlers.Admin
{
    public static class CurrencyHandlers
    {
        public static async Task<IPaginationResponse<CurrencyResponse>> GetAllWithPaginationAsync(
            [FromQuery] int? index,
            [FromQuery] RequestPagingSize? size,
            [AsParameters] CurrencyPaginatedFilterRequest currencyPaginatedFilterRequest,
            ICurrencyQueryRepository repo,
            ILogger<Currency> logger)
        {
            try
            {
                logger.LogInformation("Attempting to retrieve all currencies with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                    index, size, JsonConvert.SerializeObject(currencyPaginatedFilterRequest));

                PaginationRequest<CurrencyPaginatedFilterRequest> request = new()
                {
                    Filter = currencyPaginatedFilterRequest,
                    Paging = new()
                    {
                        Index = index ?? 0,
                        Size = size ?? 0
                    }
                };

                var result = await repo.GetCurrenciesWithPaginationAndFilterAsync(request, default);
                logger.LogInformation("Successfully retrieved {Count} currencies.", result.Base.Total);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving currencies with pagination. Index: {Index}, Size: {Size}, Filter: {Filter}",
                    index, size, JsonConvert.SerializeObject(currencyPaginatedFilterRequest));
                throw ApplicationBadRequestException.Create("Error occurred while retrieving currencies.");
            }
        }

        public static async Task<CurrencyResponse> GetByIdAsync(
            [FromRoute] int id,
            ICurrencyQueryRepository repo,
            ILogger<Currency> logger)
        {
            try
            {
                logger.LogInformation("Attempting to retrieve currency with ID: {Id}", id);

                if (id <= 0)
                {
                    logger.LogWarning("Invalid currency ID provided: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                var currency = await repo.GetByIdWithAllIncludesAsync(id);

                if (currency == null)
                {
                    logger.LogWarning("Currency with ID {Id} not found.", id);
                    throw ApplicationNotFoundException.Create($"Currency with ID: {id} not found.");
                }

                logger.LogInformation("Currency with ID {Id} successfully retrieved.", id);
                return currency;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request for currency ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Currency not found with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving currency with ID {Id}.", id);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving the currency.", (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<CurrencyResponse> GetByCodeAsync(
            [FromRoute] string code,
            ICurrencyQueryRepository repo,
            ILogger<Currency> logger)
        {
            try
            {
                logger.LogInformation("Attempting to retrieve currency with Code: {Code}", code);

                if (string.IsNullOrWhiteSpace(code))
                {
                    logger.LogWarning("Invalid currency Code provided: {Code}", code);
                    throw ApplicationBadRequestException.Create("Code is required.");
                }

                var currency = await repo.GetByCodeAsync(code, default);

                if (currency == null)
                {
                    logger.LogWarning("Currency with Code {Code} not found.", code);
                    throw ApplicationNotFoundException.Create($"Currency with Code: {code} not found.");
                }

                logger.LogInformation("Currency with Code {Code} successfully retrieved.", code);
                return currency;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request for currency Code {Code}: {ErrorMessage}", code, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Currency not found with Code {Code}: {ErrorMessage}", code, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving currency with Code {Code}.", code);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while retrieving the currency.", (int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<ResponseWrapper<CurrencyResponse>> CreateAsync(
            [FromBody] CreateCurrencyRequest request,
            ICurrencyCommandRepository repo,
            ICurrencyQueryRepository currencyQueryRepository,
            IMarketQueryRepository marketQueryRepository,
            ILogger<Currency> logger)
        {
            try
            {
                var result = new ResponseWrapper<CurrencyResponse>();
                logger.LogInformation("Attempting to create a new currency. Request: {Request}", JsonConvert.SerializeObject(request));

                request.ValidateCreateRequest(result);

                if(!result.IsSuccess)
                    return result;

                var existingCurrency = await currencyQueryRepository.GetByCodeAsync(request.Code, default);
                if (existingCurrency != null)
                {
                    logger.LogWarning("Currency creation failed: Duplicate currency code '{Code}' already exists.", request.Code);
                    result.AddError($"Currency with code '{request.Code}' already exists.");
                    return result;
                }

                var currency = Currency.Create(
                    request.Name,
                    request.Code,
                    request.Type,
                    request.CreatedById,
                    request.DecimalPrecision,
                    request.Symbol,
                    request.Published);

                if (request.MarketIds != null && request.MarketIds.Count != 0)
                {
                    foreach (var marketId in request.MarketIds)
                    {
                        var market = await marketQueryRepository.GetByIdAsync(marketId, default);

                        if (market is null)
                        {
                            logger.LogWarning("Market with ID {MarketId} not found during currency creation. Skipping addition.", marketId);
                            continue;
                        }
                        currency.AddMarket(market);
                    }
                }

                await repo.AddAsync(currency);
                var id = await repo.SaveChangesAsync(request.CreatedById, default);

                var response = new CurrencyResponse(
                    id,
                    currency.Name,
                    currency.Code,
                    currency.Type,
                    currency.CreatedById,
                    currency.Published,
                    currency.Markets?.Select(item => item.Id).ToList() ?? [],
                    currency.LastModifierUserId,
                    currency.CreatedOnUtc,
                    currency.UpdatedOnUtc,
                    currency.DecimalPrecision,
                    currency.Symbol);

                logger.LogInformation("Currency created successfully with ID {Id}.", id);
                return new ResponseWrapper<CurrencyResponse>(response);
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while creating currency: {ErrorMessage}", ex.Message);
                return new ResponseWrapper<CurrencyResponse>([$"Bad request while creating currency: {ex.Message}"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating currency. Request: {Request}", JsonConvert.SerializeObject(request));
                return new ResponseWrapper<CurrencyResponse>(["An unexpected error occurred while creating the currency."]);
            }
        }

        public static async Task<ResponseWrapper<CurrencyResponse>> UpdateAsync(
            [FromBody] CreateCurrencyRequest request,
            [FromRoute] int id,
            ICurrencyCommandRepository commandRepository,
            ICurrencyQueryRepository queryRepository,
            IMarketQueryRepository marketQueryRepository,
            ILogger<Currency> logger)
        {
            try
            {
                var result = new ResponseWrapper<CurrencyResponse>();
                logger.LogInformation("Attempting to update currency with ID: {Id}. Request: {Request}", id, JsonConvert.SerializeObject(request));

               var currency = await request.ValidateUpdateRequest(id, logger, queryRepository);

                List<Market> markets = null;
                if (request.MarketIds != null && request.MarketIds.Any())
                {
                    markets = [];
                    foreach (var marketId in request.MarketIds)
                    {
                        var market = await marketQueryRepository.GetByIdAsync(marketId, default);
                        if (market is null)
                        {
                            logger.LogWarning("Market with ID {MarketId} not found during currency update. Cannot associate with currency.", marketId);
                            result.AddError($"Market with ID: {marketId} not found. Cannot associate with currency.");
                            return result;
                        }
                        markets.Add(market);
                    }
                }

                currency.Update(request.Name, request.CreatedById, request.Published, request.DecimalPrecision, request.Symbol, markets);
                await commandRepository.UpdateAsync(currency, default, true);

                var response = new CurrencyResponse(
                    id,
                    currency.Name,
                    currency.Code,
                    currency.Type,
                    currency.CreatedById,
                    currency.Published,
                    currency.Markets?.Select(item => item.Id).ToList() ?? [],
                    currency.LastModifierUserId,
                    currency.CreatedOnUtc,
                    currency.UpdatedOnUtc,
                    currency.DecimalPrecision,
                    currency.Symbol);

                logger.LogInformation("Currency updated successfully with ID {Id}.", id);
                return new ResponseWrapper<CurrencyResponse>(response);
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while updating currency with ID {Id}: {ErrorMessage}", id, ex.Message);
                return new ResponseWrapper<CurrencyResponse>([$"Bad request while updating currency with ID {id}: {ex.Message}"]);
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Currency not found while updating with ID {Id}: {ErrorMessage}", id, ex.Message);
                return new ResponseWrapper<CurrencyResponse>([$"Bad request while updating currency with ID {id}: {ex.Message}"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating currency with ID {Id}. Request: {Request}", id, JsonConvert.SerializeObject(request));
                return new ResponseWrapper<CurrencyResponse>([$"Bad request while updating currency with ID {id}: {ex.Message}"]);
            }
        }

        public static async Task DeactivateAsync(
            [FromRoute] int id,
            ICurrencyCommandRepository commandRepository,
            ICurrencyQueryRepository queryRepository,
            ILogger<Currency> logger)
        {
            try
            {
                logger.LogInformation("Attempting to deactivate currency with ID: {Id}", id);

                if (id <= 0)
                {
                    logger.LogWarning("Invalid currency ID provided for deactivation: {Id}", id);
                    throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
                }

                var currency = await queryRepository.GetByIdAsync(id);
                if (currency == null)
                {
                    logger.LogWarning("Currency with ID {Id} not found for deactivation.", id);
                    throw ApplicationNotFoundException.Create($"Currency with ID: {id} not found.");
                }

                currency.Deactivate(1);

                var result = await commandRepository.UpdateAsync(currency, default, true);

                if (result)
                {
                    logger.LogInformation("Currency deactivated successfully with ID {Id}.", id);
                }
                else
                {
                    logger.LogWarning("Failed to deactivate currency with ID {Id}. Update operation returned false.", id);
                    throw ApplicationBadRequestException.Create("Failed to deactivate currency. The update operation was not successful.");
                }
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while deactivating currency with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (ApplicationNotFoundException ex)
            {
                logger.LogWarning(ex, "Currency not found while deactivating with ID {Id}: {ErrorMessage}", id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while deactivating currency with ID {Id}.", id);
                throw ApplicationBadRequestException.Create("An unexpected error occurred while deactivating the currency.", (int)HttpStatusCode.InternalServerError);
            }
        }

        private static void ValidateCreateRequest(this CreateCurrencyRequest request, ResponseWrapper<CurrencyResponse> response)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw ApplicationBadRequestException.Create("Currency Code is required.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw ApplicationBadRequestException.Create("Currency Name is required.");

            if (request.DecimalPrecision < 0)
                response.AddError("DecimalPrecision cannot be negative.");

            response.IsSuccess = true;
        }

        private static async Task<Currency> ValidateUpdateRequest(this CreateCurrencyRequest request, 
            int id,
            ILogger<Currency> logger, 
            ICurrencyQueryRepository queryRepository)
        {
            if (id <= 0)
            {
                logger.LogWarning("Invalid currency ID provided for update: {Id}", id);
                throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
            }

            var currency = await queryRepository.GetByIdAsync(id);
            if (currency == null)
            {
                logger.LogWarning("Illegal currency code modification attempt. CurrencyId: {CurrencyId}, OriginalCode: {OriginalCode}, AttemptedCode: {AttemptedCode}",
                id,
                currency.Code,
                request.Code);

                throw ApplicationBadRequestException.Create($"Currency code cannot be modified Code: {request.Code}.");
            }

            if (currency.Type != request.Type)
            {
                logger.LogWarning("Illegal currency Type modification attempt. CurrencyId: {CurrencyId}, OriginalType: {OriginalType}, AttemptedType: {AttemptedType}",
                    id,
                currency.Type,
                request.Type);

                throw ApplicationBadRequestException.Create($"Currency Type cannot be modified Code: {request.Type}.");
            }

            if (currency.Code != request.Code)
            {
                logger.LogWarning("Illegal currency Code modification attempt. CurrencyId: {CurrencyId}, OriginalCode: {OriginalCode}, AttemptedCode: {AttemptedCode}",
                        id,
                    currency.Code,
                    request.Code);

                throw ApplicationBadRequestException.Create($"Currency code cannot be modified Code: {request.Code}.");
            }

            if (request.DecimalPrecision < 0)
                throw ApplicationBadRequestException.Create("DecimalPrecision cannot be negative.");

            return currency;
        }
    }
}