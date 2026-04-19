using ExchangeRateProvider.Admin.Models.MarketTradingPairs;
using ExchangeRateProvider.Application.Markets.Handlers.Admin;
using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class MarketTradingPairController(
    IMarketTradingPairQueryRepository markettradingpairQuery,
    IMarketTradingPairCommandRepository markettradingpairCommand,
    ICurrencyQueryRepository currencyQueryRepository,
    IProviderQueryRepository providerQueryRepository,
    IMarketQueryRepository marketQueryRepository,
    INotifier notifier,
    ILogger<MarketTradingPair> logger) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List(
        int? marketId,
        int? currencyId,
        bool? published,
        int index = 1,
        RequestPagingSize size = RequestPagingSize.Ten,
        CancellationToken cancellationToken = default)
    {
        var filter = new MarketTradingPairPaginatedFilterRequest
        {
            MarketId = marketId,
            CurrencyId = currencyId,
            Published = published,
        };

        var paging = new RequestPaging
        {
            Index = index,
            Size = size
        };

        var request = new PaginationRequest<MarketTradingPairPaginatedFilterRequest>
        {
            Filter = filter,
            Paging = paging
        };

        var response = await markettradingpairQuery.GetMarketTradingPairsWithPaginationAndFilterAsync(request, cancellationToken);

        var model = new GetMarketTradingPairListModel
        {
            PaginationResponse = response,
            CurrencyId = currencyId,
            MarketId = marketId,
            Published = published?.ToString(),
            PublishOptions = GetPublishOptions(published),
            SizeOptions = GetPageSizeOptions((int)size),
            MarketIdOptions = await GetMarkets(null, true, cancellationToken),
            CurrencyIdOptions = await GetCurrencies(null, true, cancellationToken),
        };

        return View(model);
    }

    public async Task<IActionResult> Add(CancellationToken cancellationToken)
    {
        CreateMarketTradingPairModel model = new()
        {
            CurrencyIdOptions = await GetCurrencies(null, false, cancellationToken),
            MarketIdOptions = await GetMarkets(null, false, cancellationToken)
        };
        return View(new ResponseWrapper<CreateMarketTradingPairModel>(model));
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateMarketTradingPairModel>> Add([FromForm] ResponseWrapper<CreateMarketTradingPairModel> model, CancellationToken cancellationToken)
    {
        try
        {
            ValidateSpreadOption(model.Response.SpreadOptions);

            var reuest = new CreateMarketTradingPairRequest(
                Convert.ToInt32(model.Response.MarketId),
                Convert.ToInt32(model.Response.CurrencyId),
                model.Response.Published,
                model.Response.Description,
                null,
                GetCurrentUserId(),
                null);

            var result = await MarketTradingPairHandlers.CreateAsync(
                reuest,
                markettradingpairCommand,
                currencyQueryRepository,
                marketQueryRepository,
                markettradingpairQuery,
                logger);

            model.Data.Add("redirectUrl", Url.Action("Edit", "MarketTradingPair", new { Id = result.Id }));
            model.IsSuccess = true;

            return model;
        }
        catch (ApplicationBadRequestException ex)
        {
            model.Errors.Add(ex.Message);
            model.IsSuccess = false;

            return model;
        }
        catch (Exception)
        {
            model.Errors.Add("An unexpected error occurred.");
            model.IsSuccess = false;

            return model;
        }
    }

    public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            MarketTradingPairResponse result = await MarketTradingPairHandlers.GetByIdAsync(id, markettradingpairQuery, logger);

            List<string> selectedProviderIds = [.. result.ExchangeProviders.Select(item => item.Id.ToString())];

            CreateMarketTradingPairModel model = new()
            {
                MarketId = result.Market.Id.ToString(),
                MarketIdOptions = await GetMarkets([result.Market.Id.ToString()], false, cancellationToken),
                CurrencyId = result.CurrencyEntity.Id.ToString(),
                CurrencyIdOptions = await GetCurrencies([result.CurrencyEntity.Id.ToString()], false, cancellationToken),
                Description = result.Description,
                ExchangeRateProviderIds = selectedProviderIds,
                ExchangeRateProviderIdsOptions = await GetProviders(selectedProviderIds, cancellationToken),
                Published = result.Published,
                SpreadOptions = result.SpreadOptions,
                RatingMethod = result.RatingMethod,
                RatingMethodOptions = EnumHelper.GetEnumTypeOptions(result.RatingMethod)
            };
            return View(new ResponseWrapper<CreateMarketTradingPairModel>(model));
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
    public async Task<ResponseWrapper<CreateMarketTradingPairModel>> Edit([FromRoute] int id, ResponseWrapper<CreateMarketTradingPairModel> model, CancellationToken cancellationToken)
    {
        try
        {
            ValidateSpreadOption(model.Response.SpreadOptions);

            var reuest = new CreateMarketTradingPairRequest(
                Convert.ToInt32(model.Response.MarketId),
                Convert.ToInt32(model.Response.CurrencyId),
                model.Response.Published,
                model.Response.Description,
                model.Response.ExchangeRateProviderIds,
                GetCurrentUserId(),
                model.Response.SpreadOptions,
                model.Response.RatingMethod);

            await MarketTradingPairHandlers.UpdateAsync(
                reuest,
                id,
                markettradingpairCommand,
                markettradingpairQuery,
                providerQueryRepository,
                notifier,
                marketQueryRepository,
                currencyQueryRepository,
                logger);

            model.Data.Add("redirectUrl", Url.Action("List", "MarketTradingPair"));
            model.IsSuccess = true;

            return model;
        }
        catch (ApplicationBadRequestException ex)
        {
            model.Errors.Add(ex.Message);
            model.IsSuccess = false;

            return model;
        }
        catch (Exception)
        {
            model.Errors.Add("An unexpected error occurred.");
            model.IsSuccess = false;

            return model;
        }
    }

    [HttpPost]
    public IActionResult Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            return null;
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

    private async Task<IEnumerable<SelectListItem>> GetCurrencies(List<string> selectedIds = null, bool isListFilter = true,
        CancellationToken cancellationToken = default)
    {
        selectedIds ??= [];

        var currencies = await currencyQueryRepository.GetAllAsync(cancellationToken);

        List<SelectListItem> result = [];

        if (isListFilter)
            result.Add(new() { Value = "", Text = "All" });

        result.AddRange(currencies
            .Where(c => c.Published)
            .OrderBy(c => c.CurrencyName)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.GetCurrencyName(),
                Selected = selectedIds.Contains(c.Id.ToString())
            }));

        return result;
    }

    private async Task<IEnumerable<SelectListItem>> GetMarkets(List<string> selectedIds = null, bool isListFilter = true,
    CancellationToken cancellationToken = default)
    {
        selectedIds ??= [];

        var markets = await marketQueryRepository.GetAllPublishedMarketsWithIncludesAsync(cancellationToken);

        List<SelectListItem> result = [];

        if (isListFilter)
            result.Add(new() { Value = "", Text = "All" });

        result.AddRange(markets
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.BaseCurrency.GetCurrencyName(),
                Selected = selectedIds.Contains(c.Id.ToString())
            }));

        return result;
    }

    private async Task<IEnumerable<SelectListItem>> GetProviders(
        List<string> selectedIds = null,
        CancellationToken cancellationToken = default)
    {
        selectedIds ??= [];

        var providers = await providerQueryRepository.GetAllPublishedProvidersAsync(cancellationToken);

        return [.. providers.Select(provider => new SelectListItem
        {
            Value = provider.Id.ToString(),
            Text = provider.Name,
            Selected = selectedIds.Contains(provider.Id.ToString())
        })];
    }
}
