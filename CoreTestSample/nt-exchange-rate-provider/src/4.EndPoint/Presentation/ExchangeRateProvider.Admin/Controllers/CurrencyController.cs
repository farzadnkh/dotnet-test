using ExchangeRateProvider.Admin.Models.Currencies;
using ExchangeRateProvider.Application.Currencies.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Currencies;
using ExchangeRateProvider.Contract.Currencies.Dtos.Requests;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.Currencies.Enums;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class CurrencyController(
    ICurrencyQueryRepository currencyQuery,
    ICurrencyCommandRepository currencyCommand,
    IMarketQueryRepository marketQueryRepository,
    ILogger<Currency> logger) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List(
        string name,
        string code,
        bool? published,
        CurrencyType? type,
        int index = 1,
        RequestPagingSize size = RequestPagingSize.Ten,
        CancellationToken cancellationToken = default)
    {
        var filter = new CurrencyPaginatedFilterRequest
        {
            Name = name,
            Code = code,
            Published = published,
            Type = type
        };

        var paging = new RequestPaging
        {
            Index = index,
            Size = size
        };

        var request = new PaginationRequest<CurrencyPaginatedFilterRequest>
        {
            Filter = filter,
            Paging = paging
        };

        var response = await currencyQuery.GetCurrenciesWithPaginationAndFilterAsync(request, cancellationToken);

        var model = new GetCurrencyListModel
        {
            PaginationResponse = response,
            Name = name,
            Code = code,
            Published = published?.ToString(),
            Type = type,
            PublishOptions = GetPublishOptions(published),
            CurrencyTypesOptions = EnumHelper.GetEnumTypeOptions<CurrencyType>(),
            SizeOptions = GetPageSizeOptions((int)size)
        };

        return View(model);
    }

    public async Task<IActionResult> Add(CancellationToken cancellationToken)
    {
        CreateCurrencyModel model = new()
        {
            MarketOptions = await GetAllAsync(cancellationToken),
            CurrencyTypesOptions = EnumHelper.GetEnumTypeOptions<CurrencyType>(false),
        };
        return View(new ResponseWrapper<CreateCurrencyModel>(model));
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateCurrencyModel>> Add([FromForm] ResponseWrapper<CreateCurrencyModel> model, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateCurrencyRequest(model.Response.Name, model.Response.Code, model.Response.Type, GetCurrentUserId(), model.Response.DecimalPrecision, model.Response.Published, model.Response.Symbol, model.Response.SelectedMarketIds);
            var result = await CurrencyHandlers.CreateAsync(request, currencyCommand, currencyQuery, marketQueryRepository, logger);

            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "Currency"));
            model.IsSuccess = result.Errors.Count == 0;

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
            var result = await CurrencyHandlers.GetByIdAsync(id, currencyQuery, logger);
            CreateCurrencyModel model = new()
            {
                Code = result.Code,
                DecimalPrecision = result.DecimalPrecision,
                Name = result.Name,
                Published = result.Published,
                Symbol = result.Symbol,
                Type = result.Type,
                CurrencyTypesOptions = EnumHelper.GetEnumTypeOptions(result.Type),
                MarketOptions = await GetAllAsync(cancellationToken),
                SelectedMarketIds = result.MarketIds
            };
            return View(new ResponseWrapper<CreateCurrencyModel>(model));
        }
        catch (ApplicationBadRequestException ex)
        {
            return View(new ResponseWrapper<CreateCurrencyModel>([$"Bad request while getting currency: {ex.Message}"]));
        }
        catch (Exception ex)
        {
            return View(new ResponseWrapper<CreateCurrencyModel>([$"Bad request while getting currency: {ex.Message}"]));
        }
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateCurrencyModel>> Edit([FromRoute] int id, ResponseWrapper<CreateCurrencyModel> model, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateCurrencyRequest(model.Response.Name, model.Response.Code, model.Response.Type, GetCurrentUserId(), model.Response.DecimalPrecision, model.Response.Published, model.Response.Symbol, model.Response.SelectedMarketIds);
            var result = await CurrencyHandlers.UpdateAsync(request, id, currencyCommand, currencyQuery, marketQueryRepository, logger);

            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "Currency"));
            model.IsSuccess = !result.Errors.Any();

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

    private async Task<IEnumerable<SelectListItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        var markets = await marketQueryRepository.GetAllMarketsWithIncludesAsync(cancellationToken);

        var result = new List<SelectListItem>
        {
            new() { Value = "", Text = "" }
        };

        foreach (var market in markets)
            result.Add(new() { Value = market.BaseCurrency.Id.ToString(), Text = market.BaseCurrency.Code });

        return result;
    }
}
