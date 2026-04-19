using ExchangeRateProvider.Admin.Models.ManualRatings;
using ExchangeRateProvider.Application.Markets.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons.Services;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Admin.Controllers
{
    public class ManualRatingController(
        IMarketTradingPairQueryRepository tradingPairQueryRepository,
        IRedisService redisService,
        IRedisDatabase redisDatabase,
        ISettingQueryRepository settingQueryRepository,
        ILogger<MarketTradingPair> logger) : BaseController
    {
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var result = await ManualRatingHandler.GetAllAsync(redisDatabase, tradingPairQueryRepository, logger, cancellationToken);

            return View(new ManualRatingListModel()
            {
                ManualRatingResponses = result,
            });
        }

        [HttpPost]
        public async Task<ResponseWrapper<string>> Save([FromBody] ManualRatingRequest model, CancellationToken cancellationToken)
        {
            try
            {
                var response = new ResponseWrapper<string>();
                var result = await ManualRatingHandler.UpsertRateAsync(model, tradingPairQueryRepository, redisService, redisDatabase, settingQueryRepository, logger, cancellationToken);
                if (result.Response)
                {
                    response.IsSuccess = true;
                    response.Response = $"Pair: {model.TradingPair} successfully Updated.";
                    return response;
                }

                response.IsSuccess = false;
                response.AddError(result.Errors.FirstOrDefault());
                return response;
            }
            catch (ApplicationBadRequestException ex)
            {
                ResponseWrapper<string> result = new();
                result.IsSuccess = false;
                return new([ex.Message]);
            }
            catch (Exception ex)
            {
                ResponseWrapper<string> result = new();
                result.IsSuccess = false;
                return new(["Something went Wrong"]);
            }
        }
    }
}
