using Microsoft.AspNetCore.Mvc;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Models.Responses;
using NT.SDK.ExchangeRateProvider.Services.StreamRateServices;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace NT.SDK.ExchangeRateProvider.Sample.Controllers;

[ApiController]
public class PriceController(
    IExchangeRateProviderStreamRateService rateService,
    IRedisDatabase redisDatabase,
    ExchangeRateProviderOptions options) : Controller
{
    /// <summary>
    /// "This is a simple sample API for retrieving prices. 
    /// We assume that you have already configured the sockets or started the job responsible for fetching and storing data in Redis."
    /// </summary>
    /// <param name="tradingPair"></param>
    /// <returns></returns>
    [HttpGet("get_prices")]
    public async Task<IActionResult> GetPrices(
        [FromQuery]
        [Description("This is your key in Redis, and it is generated like this: 'BTCUSDT'.")] string tradingPair)
    {
        try
        {
            // This is how we generate Redis Keys.
            var pairKey = RedisKeys.GetStoredPriceKey(options.CachePrefix, tradingPair);

            if (await redisDatabase.ExistsAsync(pairKey))
                return Ok(await redisDatabase.GetAsync<GetLatestPriceResponse>(pairKey));
            else
                return NotFound($"There is no Stored Price For Requested Pair: {tradingPair}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// This is sample of how we use our socket services
    /// </summary>
    /// <param name="tradingPair"></param>
    /// <returns></returns>
    [HttpPost("socket/connect")]
    public async Task<IActionResult> ConnectSocket()
    {
        try
        {
            await rateService.StartStreamAsync(new GetTokenRequest()
            {
                ClientId = "ZzhFkoKfr4o_GbiKTqMduob1xhOQeSnXfhrf2bq1lJBMmocAiOgWAC3B8M6hN305",
                ClientSecret = "tHrMwaSbRfvUgexY3hRTG5mFDxMUgRYRb77+rf2LZq7UvvbX7BlG4onf4w7NKINHntaByMogCQ9FYz1iY82pvA==",
                Scopes = "realtime-api"
            }, renewToken: true, cancellationToken: default);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// This is sample of how we use our socket services
    /// </summary>
    /// <param name="tradingPair"></param>
    /// <returns></returns>
    [HttpPost("socket/send-message")]
    public async Task<IActionResult> SendMessageToSocket([AllowNull][FromQuery] GetLatestPriceRequest request)
    {
        try
        {
            await rateService.SendMessageAsync(request, cancellationToken: default);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
