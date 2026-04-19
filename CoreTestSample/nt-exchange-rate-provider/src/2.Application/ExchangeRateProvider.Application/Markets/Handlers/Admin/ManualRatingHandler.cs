using ExchangeRateProvider.Application.Consumers.Extensions;
using ExchangeRateProvider.Application.Settings.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Dtos;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.Commons.Services;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Settings.Entities;
using ExchangeRateProvider.Domain.Settings.Enums;
using MessagePack;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Threading.Tasks;

namespace ExchangeRateProvider.Application.Markets.Handlers.Admin;

public static class ManualRatingHandler
{
    #region Methods

    public async static Task<List<ManualRatingResponse>> GetAllAsync(
    IRedisDatabase redisDatabase,
    IMarketTradingPairQueryRepository marketTradingPairQueryRepository,
    ILogger<MarketTradingPair> logger,
    CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllAsync with cancellationToken: {CancellationToken}", cancellationToken);

        try
        {
            var manualPairs = await marketTradingPairQueryRepository.GetAllManualTradingParisAsync(cancellationToken);
            logger.LogInformation("Retrieved {Count} manual trading pairs", manualPairs?.Count ?? 0);

            if (manualPairs == null || manualPairs.Count <= 0)
            {
                logger.LogWarning("No manual trading pairs found");
                return new();
            }

            List<ManualRatingResponse> response = new();
            foreach (var pair in manualPairs)
            {
                try
                {
                    logger.LogDebug("Processing pair: {PairId}", pair.Id);

                    var pairSnapShot = pair.ToSnapshot([.. pair.MarketTradingPairProviders.Select(item => item.ExchangeRateProviderId)]);
                    var hashKey = RedisKeys.GenerateTradingPairKey(pairSnapShot.BaseCurrencyCode, pairSnapShot.CurrencyCode);

                    logger.LogDebug("Generated keys - hashKey: {HashKey}", hashKey);

                    var spreadOptions = new SpreadOptionStruct(null, null, pairSnapShot, pair?.Market).GetActiveSpreadOption();

                    var packedPrice = await redisDatabase.HashGetAllAsync<byte[]>(hashKey);
                    if (packedPrice.Count <= 0)
                    {
                        logger.LogWarning("No price data found in Redis for pair: {PairId}", pair.Id);
                        response.Add(new()
                        {
                            TradingPair = $"{pairSnapShot.CurrencyCode}{pairSnapShot.BaseCurrencyCode}",
                            TradingPairId = pair.Id,
                            Price = 0,
                            LowerLimit = 0,
                            UpperLimit = 0,
                            OldPrice = 0,
                            LowerLimitPercentage = spreadOptions?.LowerLimitPercentage ?? 0,
                            UpperLimitPercentage = spreadOptions?.UpperLimitPercentage ?? 0
                        });
                        continue;
                    }

                    ExchangeRateValue unPackedPrice;
                    try
                    {
                        unPackedPrice = MessagePackSerializer.Deserialize<ExchangeRateValue>(packedPrice.FirstOrDefault().Value);
                        logger.LogDebug("Deserialized price for pair {PairId}: {Price}", pair.Id, unPackedPrice.Price);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to deserialize price data for pair {PairId}", pair.Id);
                        continue;
                    }

         

                    var (upperLimit, lowerLimit) = spreadOptions.GetSpreadPrice(unPackedPrice.Price);
                    logger.LogDebug("Calculated spread for pair {PairId} - Upper: {UpperLimit}, Lower: {LowerLimit}",
                        pair.Id, upperLimit, lowerLimit);

                    response.Add(new ManualRatingResponse
                    {
                        TradingPairId = pair.Id,
                        Price = unPackedPrice.Price,
                        LowerLimit = lowerLimit,
                        UpperLimit = upperLimit,
                        TradingPair = $"{pairSnapShot.CurrencyCode}{pairSnapShot.BaseCurrencyCode}",
                        OldPrice = 0,
                        LowerLimitPercentage = spreadOptions?.LowerLimitPercentage ?? 0,
                        UpperLimitPercentage = spreadOptions?.UpperLimitPercentage ?? 0
                    });
                    logger.LogDebug("Added response for pair {PairId}", pair.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing pair {PairId}", pair.Id);
                    continue;
                }
            }

            logger.LogInformation("Returning {Count} manual rating responses", response.Count);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetAllAsync");
            throw;
        }
    }

    public async static Task<ResponseWrapper<bool>> UpsertRateAsync(
     ManualRatingRequest request,
     IMarketTradingPairQueryRepository marketTradingPairQueryRepository,
     IRedisService redisService,
     IRedisDatabase redisDatabase,
     ISettingQueryRepository settingQueryRepository,
     ILogger<MarketTradingPair> logger,
     CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpsertRateAsync for trading pair: {TradingPair}, price: {Price}, cancellationToken: {CancellationToken}",
            request.TradingPair, request.NewPrice, cancellationToken);

        ResponseWrapper<bool> response = new();

        try
        {
            logger.LogDebug("Validating request for trading pair: {TradingPair}", request.TradingPair);
            await request.ValidateRequest(logger);

            var pair = await marketTradingPairQueryRepository.GetByIdWithAllIncludesAsync(request.PairId, cancellationToken);
            if (pair == null)
            {
                logger.LogWarning("Trading pair not found for ID: {PairId}", request.PairId);
                response.AddError("Trading pair not found");
                return response;
            }


            logger.LogDebug("Retrieved pair {PairId} with rating method: {RatingMethod}", pair.Id, pair.RatingMethod);

            if (pair.RatingMethod == RatingMethod.Manual)
            {
                try
                {
                    var providerType = pair.MarketTradingPairProviders.FirstOrDefault().ExchangeRateProvider.Type;
                    logger.LogDebug("Saving price data to Redis for pair {PairId}, provider type: {ProviderType}", pair.Id, providerType);

                    var pairSnapShot = pair.ToSnapshot([.. pair.MarketTradingPairProviders.Select(item => item.ExchangeRateProviderId)]);
                    var hashKey = RedisKeys.GenerateTradingPairKey(pairSnapShot.BaseCurrencyCode, pairSnapShot.CurrencyCode);

                    logger.LogDebug("Generated keys - hashKey: {HashKey}", hashKey);

                    var packedPrice = await redisDatabase.HashGetAllAsync<byte[]>(hashKey);

                    if(packedPrice.Count() > 0)
                    {
                        var rateRangePercentage = await SettingHandler.GetSettingByNameAsync(SettingName.ManualRateChangePercentage, settingQueryRepository, default);

                        ExchangeRateValue unPackedPrice;
                        try
                        {
                            unPackedPrice = MessagePackSerializer.Deserialize<ExchangeRateValue>(packedPrice.FirstOrDefault().Value);
                            logger.LogDebug("Deserialized price for pair {PairId}: {Price}", pair.Id, unPackedPrice.Price);

                            request.OldPrice = unPackedPrice.Price;

                            if (!IsNewRateInRange(rateRangePercentage, request.NewPrice, request.OldPrice))
                            {
                                logger.LogWarning("Validation failed: Price {Price} is not In A valid Range", request.NewPrice);
                                throw ApplicationBadRequestException.Create("New Price Is not In a Valid Range.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to deserialize price data for pair {PairId}", pair.Id);
                            throw;
                        }

                        await redisDatabase.RemoveAsync(hashKey);
                    }

                    await redisService.SavePairsPriceDataToRedisAsync(
                        providerType,
                        request.TradingPair,
                        request.NewPrice ?? 0,
                        0,
                        null);

                    logger.LogInformation("Successfully saved price data for pair {PairId}", pair.Id);
                    response.Response = true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                logger.LogWarning("Invalid rating method for pair {PairId}: {RatingMethod}", pair.Id, pair.RatingMethod);
                response.AddError("Invalid rating method for this pair");
            }

            return response;
        }
        catch (ApplicationBadRequestException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    #endregion

    #region Utilities
    private static async Task ValidateRequest(this ManualRatingRequest request, ILogger<MarketTradingPair> logger)
    {
        logger.LogInformation("Validating ManualRatingRequest for trading pair: {TradingPair}", request.TradingPair);

        try
        {
            if (string.IsNullOrWhiteSpace(request.TradingPair))
            {
                logger.LogWarning("Validation failed: Trading pair is empty or null");
                throw ApplicationBadRequestException.Create("Pair Name is Required.");
            }

            if (request.NewPrice <= 0)
            {
                logger.LogWarning("Validation failed: Price {Price} is less than or equal to 0", request.NewPrice);
                throw ApplicationBadRequestException.Create("Price Could Not be Less Than 0");
            }

            logger.LogDebug("Request validation successful for trading pair: {TradingPair}", request.TradingPair);
           
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Validation error for trading pair: {TradingPair}", request.TradingPair);
            throw;
        }
    }

    private static bool IsNewRateInRange(Setting rangLimiPercentageSetting, decimal? newPrice, decimal? oldPrice)
    {
        if (newPrice is null || newPrice == 0)
            return true;

        if (rangLimiPercentageSetting is null || rangLimiPercentageSetting.Value == "0")
            return true;

        var isValidPercentageSettingFormat = decimal.TryParse(rangLimiPercentageSetting.Value, out var rangLimiPercentage);

        if (!isValidPercentageSettingFormat)
            return false;

        if (oldPrice is null || oldPrice == 0)
            return true;

        var upperLimitPercentage = oldPrice + MathHelper.Ratio(oldPrice * rangLimiPercentage ?? 0, 100);
        var lowerLimitPercentage = oldPrice - MathHelper.Ratio(oldPrice * rangLimiPercentage ?? 0, 100);

        if (upperLimitPercentage < newPrice)
            return false;
        if (lowerLimitPercentage > newPrice)
            return false;

        return true;
    }
    #endregion
}
