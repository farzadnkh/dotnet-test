using DotNet.Testcontainers.Clients;
using ExchangeRateProvider.Admin.Models.Providers;
using ExchangeRateProvider.Application.ExchangeRateProviders.Handlers.Admin;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class ProviderController(
    IProviderQueryRepository providerQuery,
    IProviderCommandRepository providerCommand,
    INotifier notifier,
    ILogger<Provider> logger) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List(
        string name,
        bool? published,
        ProviderType provider,
        int index = 1,
        RequestPagingSize size = RequestPagingSize.Ten,
        CancellationToken cancellationToken = default)
    {
        var filter = new ProviderPaginatedFilterRequest
        {
            Name = name,
            Published = published,
            ProviderType = provider
        };

        var paging = new RequestPaging
        {
            Index = index,
            Size = size
        };

        var request = new PaginationRequest<ProviderPaginatedFilterRequest>
        {
            Filter = filter,
            Paging = paging
        };

        var response = await providerQuery.GetProvidersWithPaginationAndFilterAsync(request, cancellationToken);

        var model = new GetProviderListModel
        {
            PaginationResponse = response,
            Name = name,
            Published = published?.ToString(),
            PublishOptions = GetPublishOptions(published),
            SizeOptions = GetPageSizeOptions((int)size),
            ProviderTypesOptions = EnumHelper.GetEnumTypeOptions<ProviderType>(),
        };

        return View(model);
    }

    public IActionResult Add()
    {
        CreateProviderModel model = new()
        {
            ProviderTypesOptions = EnumHelper.GetEnumTypeOptions<ProviderType>(false)
        };
        return View(new ResponseWrapper<CreateProviderModel>(model));
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateProviderModel>> Add(ResponseWrapper<CreateProviderModel> model, CancellationToken cancellationToken)
    {
        try
        {
            var selectedMarkets = JsonConvert.DeserializeObject<List<string>>(model.Response.SelectedMarkets.ElementAt(0));
            var request = new CreateProviderRequest(model.Response.Name, model.Response.ProviderType, model.Response.Published, 1, selectedMarkets);
            var result = await ProviderHandlers.CreateAsync(request, providerCommand, providerQuery, logger);

            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "Provider"));
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

    public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await ProviderHandlers.GetByIdAsync(id, providerQuery, logger);
            EditProviderModel model = new()
            {
                Id = result.Id,
                ProviderType = result.Type,
                Name = result.Name,
                Published = result.Published,
                Markets = GetProviderMarkets(result.Type),
                SelectedMarkets = result.SelectedMarkets,
                ProviderTypesOptions = EnumHelper.GetEnumTypeOptions(result.Type)
            };
            return View(new ResponseWrapper<EditProviderModel>(model));
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
    public async Task<ResponseWrapper<EditProviderModel>> Edit(ResponseWrapper<EditProviderModel> model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return model;
        }
        try
        {
            var markets = model.Response.SelectedMarkets.ElementAt(0) == null ? "" : model.Response.SelectedMarkets.ElementAt(0);
            var selectedMarkets = JsonConvert.DeserializeObject<List<string>>(markets);
            var request = new CreateProviderRequest(model.Response.Name, model.Response.ProviderType, model.Response.Published, GetCurrentUserId(), selectedMarkets);
            var result = await ProviderHandlers.UpdateAsync(request, model.Response.Id, providerCommand, providerQuery, notifier, logger);

            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "Provider"));
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

    [HttpGet]
    public List<string> GetProviderMarkets(ProviderType providerType)
    {
        if (providerType == ProviderType.CryptoCompare)
        {
            return [.. Enum.GetNames<CryptoCompareBusinessName>()];
        }

        return [];
    }
}
