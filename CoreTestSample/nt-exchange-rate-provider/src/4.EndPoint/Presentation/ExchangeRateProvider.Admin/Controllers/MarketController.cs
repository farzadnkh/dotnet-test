using ExchangeRateProvider.Admin.Models.Markets;
using ExchangeRateProvider.Application.Markets.Handlers.Admin;
using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.Markets.Dtos.Requests;
using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class MarketController(
    IMarketQueryRepository marketQuery,
    IMarketCommandRepository marketCommand,
    ICurrencyQueryRepository currencyQueryRepository,
    IProviderQueryRepository providerQueryRepository,
    IMarketTradingPairCommandRepository tradingPairCommandRepository,
    INotifier notifier,
    ILogger<Market> logger) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> List(
        int? currencyId,
        MarketCalculationTerm? term,
        bool? published,
        int index = 1,
        RequestPagingSize size = RequestPagingSize.Ten,
        CancellationToken cancellationToken = default)
    {
        var filter = new MarketPaginatedFilterRequest
        {
            CurrencyId = currencyId,
            MarketCalculationTerm = term,
            Published = published,
        };

        var paging = new RequestPaging
        {
            Index = index,
            Size = size
        };

        var request = new PaginationRequest<MarketPaginatedFilterRequest>
        {
            Filter = filter,
            Paging = paging
        };

        var response = await marketQuery.GetMarketsWithPaginationAndFilterAsync(request, cancellationToken);

        var model = new GetMarketListModel
        {
            PaginationResponse = response,
            CurrencyId = currencyId,
            Published = published?.ToString(),
            PublishOptions = GetPublishOptions(published),
            SizeOptions = GetPageSizeOptions((int)size),
            CurrencyIdOptions = await GetCurrencies(null, true, cancellationToken),
            CalculationOptions = EnumHelper.GetEnumTypeOptions<MarketCalculationTerm>()
        };

        return View(model);
    }

    public async Task<IActionResult> Add(CancellationToken cancellationToken)
    {
        CreateMarketModel model = new()
        {
            MarketCurrencyIdsOptions = await GetCurrencies(null, false, cancellationToken)
        };
        return View(new ResponseWrapper<CreateMarketModel>(model));
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateMarketModel>> Add([FromForm] ResponseWrapper<CreateMarketModel> model, CancellationToken cancellationToken)
    {
        try
        {
            ValidateSpreadOption(model.Response.SpreadOptions);

            var reuest = new CreateMarketRequest(
                Convert.ToInt32(model.Response.MarketCurrencyId ?? "0"),
                null,
                model.Response.IsDefault,
                true,
                model.Response.CreateAllFiats,
                model.Response.CreateAllCryptos,
                [],
                GetCurrentUserId());
            var result = await MarketHandlers.CreateAsync(
                reuest,
                marketCommand,
                marketQuery,
                tradingPairCommandRepository,
                currencyQueryRepository,
                logger);

            model.Data.Add("redirectUrl", Url.Action("Edit", "Market" , new { Id = result.Id} ));
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
            MarketResponse result = await MarketHandlers.GetByIdAsync(id, marketQuery, logger);

            List<string> selectedProviderIds = [.. result.ExchangeProviders.Select(item => item.Id.ToString())];

            CreateMarketModel model = new()
            {
                MarketCurrencyId = result.BaseCurrencyId.ToString(),
                MarketCurrencyIdsOptions = await GetCurrencies([result.BaseCurrencyId.ToString()], false, cancellationToken),
                MarketCalculationTerms = result.Term,
                IsDefault = result.IsDefault,
                Published = result.Published,
                SpreadOptions = result.SpreadOptions,
                ExchangeRateProviderIds = selectedProviderIds,
                CalculationOptions = EnumHelper.GetEnumTypeOptions(result.Term.Value),
                ExchangeRateProviderIdsOptions = await GetProviders(selectedProviderIds, cancellationToken),
                RatingMethodOptions = EnumHelper.GetEnumTypeOptions(result.RatingMethod),
                RatingMethod = result.RatingMethod
            };
            return View(new ResponseWrapper<CreateMarketModel>(model));
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
    public async Task<ResponseWrapper<CreateMarketModel>> Edit([FromRoute] int id, ResponseWrapper<CreateMarketModel> model, CancellationToken cancellationToken)
    {
        try
        {
            ValidateSpreadOption(model.Response.SpreadOptions);

            var reuest = new CreateMarketRequest(
                Convert.ToInt32(model.Response.MarketCurrencyId),
                model.Response.MarketCalculationTerms,
                model.Response.IsDefault,
                model.Response.Published,
                model.Response.CreateAllFiats,
                model.Response.CreateAllCryptos,
                model.Response.ExchangeRateProviderIds,
                GetCurrentUserId(),
                model.Response.SpreadOptions,
                model.Response.RatingMethod);
            var result = await MarketHandlers.UpdateAsync(reuest, id, marketCommand, marketQuery, providerQueryRepository, tradingPairCommandRepository, notifier, logger);

            model.Data.Add("redirectUrl", Url.Action("List", "Market"));
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

    private async Task<IEnumerable<SelectListItem>> GetProviders(
        List<string> selectedIds = null,
        CancellationToken cancellationToken = default)
    {
        selectedIds ??= [];

        var providers = await providerQueryRepository.GetAllPublishedProvidersAsync(cancellationToken);

        return [.. providers.Select(provider => new SelectListItem
        {
            Value = provider.Id.ToString(),
            Text = provider.Name.ToString(),
            Selected = selectedIds.Contains(provider.Id.ToString())
        })];
    }
}
