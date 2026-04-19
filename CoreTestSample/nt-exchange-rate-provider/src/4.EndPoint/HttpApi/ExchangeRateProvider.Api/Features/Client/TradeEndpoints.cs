using Duende.IdentityServer.Validation;
using ExchangeRateProvider.Application.Commons.Helpers;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NT.DDD.Presentation.ApiResponses;
using NT.DDD.Presentation.Exceptions;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace ExchangeRateProvider.Api.Features.Client
{
    public static class TradeEndpoints
    {
        #region EndPoints
        public static void MapTradeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/app/pairs")
                .WithOpenApi()
                .WithTags("Pairs")
                .WithGroupName(SwaggerConstatns.ThirdPartyDefinition)
                .RequireCors("AllowAnyOrigin");

            group.MapGet("latest/price", [Authorize] async (
                [AsParameters] GetLatestTickRequest request,
                [FromServices] IAggregatorServiceFactory aggregatorServiceFactory,
                [FromServices] IRedisDatabase redisDatabase,
                [FromServices] EncryptionConfiguration encryption,
                ITokenValidator tokenValidator,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var consumerId = await GetConsumerId(tokenValidator, context.GetToken(), encryption.EncryptionKey);

                var cache = await GetFromCacheAsync(redisDatabase, consumerId);
                if (cache is not null)
                    return GenerateResult(cache);

                var priceUpdatesAsync = aggregatorServiceFactory.GetPairExchangeRatesForConsumerAllPairsAsync(consumerId, request.CreateFilterDto(), true, cancellationToken);
                Dictionary<string, PriceDetails> aggregatedPrices = await GenerateAggregateRusltAsync(priceUpdatesAsync, request, redisDatabase, consumerId, cancellationToken);
                return GenerateResult(aggregatedPrices);
            })
                .Produces<APIResponse<AggregatedPricesDto>>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
                .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
                .WithDescription("Get latest Tick Price.");
        }
        #endregion

        #region Utilities
        private static IResult GenerateResult(Dictionary<string, PriceDetails> aggregatedPrices)
        {
            var resultDto = new AggregatedPricesDto();
            foreach (var entry in aggregatedPrices)
            {
                resultDto[entry.Key] = entry.Value;
            }

            return Results.Ok(resultDto);
        }

        private static async Task<Dictionary<string, PriceDetails>> GenerateAggregateRusltAsync(
            IAsyncEnumerable<PairExchangeRateDto> priceUpdatesAsync,
            GetLatestTickRequest request,
            IRedisDatabase redisDatabase,
            int consumerId,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, PriceDetails>();
            await foreach (var update in priceUpdatesAsync.WithCancellation(cancellationToken))
            {
                if (update?.CurrencyPair == null) continue;

                if (!result.TryGetValue(update.CurrencyPair, out var details))
                {
                    details = new PriceDetails
                    {
                        Price = update.Price,
                        Ask = update.UpperLimit,
                        Bid = update.LowerLimit,
                        BidSpreadPercentage = update.LowerLimitPercentage,
                        AskSpreadPercentage = update.UpperLimitPercentage,
                    };
                    result.Add(update.CurrencyPair, details);
                }
                else
                {
                    details.Price = update.Price;
                    details.Ask = update.UpperLimit;
                    details.Bid = update.LowerLimit;
                    details.BidSpreadPercentage = update.LowerLimitPercentage;
                    details.AskSpreadPercentage = update.UpperLimitPercentage;
                }
            }

            if (request.EnableCache is not null && request.EnableCache.Value == true)
                await redisDatabase.AddAsync(RedisKeys.GenerateTradingPairApiCacheKey(consumerId), result, expiresIn: TimeSpan.FromSeconds(request.CacheTtlInSec ?? 10));

            return result;
        }

        private static GetExchangeRateForAllPairsWithFilter CreateFilterDto(this GetLatestTickRequest request)
        {
            if (request == null) return null;

            GetExchangeRateForAllPairsWithFilter result = new();

            if (!string.IsNullOrWhiteSpace(request.Market))
                result.Markets = [.. request?.Market?.Trim().Split(',')];

            if (!string.IsNullOrWhiteSpace(request.Pairs))
                result.Pairs = [.. request?.Pairs?.Trim().Split(',')];

            result.ProviderTypes = request.ProviderTypes ?? ProviderType.None;
            return result;
        }

        private static async Task<Dictionary<string, PriceDetails>> GetFromCacheAsync(IRedisDatabase redisDatabase, int consumerId)
            => await redisDatabase.GetAsync<Dictionary<string, PriceDetails>>(RedisKeys.GenerateTradingPairApiCacheKey(consumerId));

        private static async Task<int> GetConsumerId(ITokenValidator tokenValidator, string token, string encryptionKey)
        {
            if (string.IsNullOrEmpty(token))
                throw PresentationBadRequestException.Create("Access Token Is Invalid");

            var validation = await tokenValidator.ValidateAccessTokenAsync(token);

            if (validation.IsError)
                throw PresentationBadRequestException.Create("Access Token Is Invalid");

            var clientId = validation.Client?.ClientId;

            return TokenHelper.GetConsumerId(clientId, encryptionKey);
        }

        #endregion
    }

    public class PriceDetails
    {
        public decimal Price { get; set; }
        public decimal? Ask { get; set; }
        public decimal? AskSpreadPercentage { get; set; }
        public decimal? Bid { get; set; }
        public decimal? BidSpreadPercentage { get; set; }
    }

    public class AggregatedPricesDto : Dictionary<string, PriceDetails>
    {
    }
}
