using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Admin.Models.ConsumerConfiguration;
using ExchangeRateProvider.Application.Consumers.Handlers.Admin;
using ExchangeRateProvider.Domain.Consumers.Entities;


namespace ExchangeRateProvider.Admin.Controllers
{
    [Authorize(AuthenticationSchemes = "oidc")]
    public class ConsumerConfigurationController(
        IConsumerCommandRepository consumerCommand,
        IConsumerQueryRepository consumerQuery,
        IMarketQueryRepository marketQuery,
        IProviderQueryRepository providerQuery,
        IMarketTradingPairQueryRepository tradingPairQueryRepository,
        INotifier notifier,
        ILogger<Consumer> logger) : BaseController
    {
        public async Task<IActionResult> Add(CancellationToken cancellationToken)
        {
            return View(new CreateConsumerConfigurationModel()
            {
                ProviderOptions = await GetProviders(isListFilter: false, cancellationToken: cancellationToken),
                MarketOptions = await GetMarkets(isListFilter: false, cancellationToken: cancellationToken),
                PairOptions = await GetPairs(isListFilter: false, cancellationToken: cancellationToken),
                ConsumerOptions = await GetConsumers(isListFilter: true, cancellationToken: cancellationToken)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] CreateConsumerConfigurationModel model, [FromForm] string actionType, CancellationToken cancellationToken)
        {
            try
            {
                ValidateSpreadOption(model.SpreadOptions);
                var reuest = new CreateConsumerConfigurationRequest(
                    Convert.ToInt32(model.ConsumerId),
                    model.IsActive,
                    null,
                    GetCurrentUserId(),
                    model.ProviderIds,
                    model.MarketIds,
                    model.PairIds);

                var result = await ConsumerConfigurationHandlers.CreateAsync(reuest, consumerCommand, consumerQuery, tradingPairQueryRepository, marketQuery, logger);

                if (!string.IsNullOrWhiteSpace(actionType) && actionType == ActionType.CreateAjax.ToString())
                    return Ok();

                return RedirectToAction("List");
            }
            catch (ApplicationBadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await ConsumerConfigurationHandlers.GetByIdAsync(id, consumerQuery, logger);

                List<string> selectedProviderIds = [.. result.ExchangeProviders.Select(item => item.Id.ToString())];
                List<string> selectedMarketIds = [.. result.Markets.Select(item => item.Id.ToString())];
                List<string> selectedPairIds = [.. result.TradingPairs.Select(item => item.Id.ToString())];

                CreateConsumerConfigurationModel model = new()
                {
                    ProviderOptions = await GetProviders(selectedProviderIds, isListFilter: false, cancellationToken: cancellationToken),
                    MarketOptions = await GetMarkets(selectedMarketIds, isListFilter: false, cancellationToken: cancellationToken),
                    PairOptions = await GetPairs(selectedPairIds, isListFilter: false, cancellationToken: cancellationToken),
                    ConsumerOptions = await GetConsumers([result.ConsumerId.ToString()], isListFilter: false, cancellationToken: cancellationToken)
                };
                return View(model);
            }
            catch (ApplicationBadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] CreateConsumerConfigurationModel model, CancellationToken cancellationToken)
        {
            try
            {
                ValidateSpreadOption(model.SpreadOptions);
                var providerIds = JsonConvert.DeserializeObject<List<string>>(model.ProviderIds.ElementAt(0));
                var marketIds = JsonConvert.DeserializeObject<List<string>>(model.MarketIds.ElementAt(0));
                var pairIds = JsonConvert.DeserializeObject<List<string>>(model.PairIds.ElementAt(0));

                var request = new CreateConsumerConfigurationRequest(model.ConsumerId, model.IsActive, model.SpreadOptions, GetCurrentUserId(), providerIds, marketIds, pairIds);
                var result = await ConsumerConfigurationHandlers.UpdateAsync(request, consumerCommand, consumerQuery, tradingPairQueryRepository, marketQuery, notifier, logger);
                if (result is not null)
                {
                    return Ok(new { success = true, message = "Configuration updated successfully." });
                }
                else
                {
                    return BadRequest(new { success = false, error = "Failed to update configuration." });
                }
            }
            catch (ApplicationBadRequestException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        #region Utilites
        public async Task<IEnumerable<SelectListItem>> GetProviders(
            List<string> selectedIds = null,
            List<string> selectedMarketIds = null,
            List<string> selectedPairIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];
            selectedMarketIds ??= [];
            selectedPairIds ??= [];

            var selectedMarketIdInts = selectedMarketIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();
            var selectedPairIdInts = selectedPairIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();

            var providers = await providerQuery.GetAllPublishedProvidersAsync(cancellationToken);

            if (selectedMarketIdInts.Count > 0 || selectedPairIdInts.Count > 0)
            {
                providers = [.. providers
                    .Where(p =>
                        (selectedMarketIdInts.Count == 0 ||
                            p.MarketExchangeRateProviders.Any(mp => selectedMarketIdInts.Contains(mp.MarketId)))
                        &&
                        (selectedPairIdInts.Count == 0 ||
                            p.MarketTradingPairProviders.Any(pp => selectedPairIdInts.Contains(pp.MarektTradingPairId)))
                    )];
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

        public async Task<IEnumerable<SelectListItem>> GetMarkets(
            List<string> selectedIds = null,
            List<string> selectedProviderIds = null,
            List<string> selectedPairIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];
            selectedProviderIds ??= [];
            selectedPairIds ??= [];

            var selectedProviderIdInts = selectedProviderIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();
            var selectedPairIdInts = selectedPairIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();

            var markets = await marketQuery.GetAllPublishedMarketsWithIncludesAsync(cancellationToken);

            if (selectedProviderIdInts.Count != 0)
            {
                markets = [.. markets.Where(m => m.MarketExchangeRateProviders.Any(p => selectedProviderIdInts.Contains(p.ExchangeRateProviderId)))];
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

        public async Task<IEnumerable<SelectListItem>> GetPairs(
            List<string> selectedIds = null,
            List<string> selectedMarketIds = null,
            List<string> selectedProviderIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];
            selectedMarketIds ??= [];
            selectedProviderIds ??= [];

            var selectedMarketIdInts = selectedMarketIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();
            var selectedProviderIdInts = selectedProviderIds.Where(id => int.TryParse(id, out _)).Select(int.Parse).ToHashSet();

            var pairs = await tradingPairQueryRepository.GetAllPublishedMarketTradingPairsWithIncludesAsync(cancellationToken);

            if (selectedMarketIdInts.Count != 0)
                pairs = [.. pairs.Where(p => selectedMarketIdInts.Contains(p.MarketId))];

            if (selectedProviderIdInts.Count != 0)
                pairs = [.. pairs.Where(p => p.Market.MarketExchangeRateProviders.Any(e => selectedProviderIdInts.Contains(e.ExchangeRateProviderId)))];

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

        private async Task<IEnumerable<SelectListItem>> GetConsumers(
            List<string> selectedIds = null,
            bool isListFilter = false,
            CancellationToken cancellationToken = default)
        {
            selectedIds ??= [];

            var consumers = await consumerQuery.GetAllWithAllIncludesAsync(cancellationToken);

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
        #endregion
    }
}
