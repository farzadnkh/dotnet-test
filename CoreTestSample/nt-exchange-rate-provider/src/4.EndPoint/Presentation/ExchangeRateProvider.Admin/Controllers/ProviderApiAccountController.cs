using ExchangeRateProvider.Admin.Models.ProviderApiAccounts;
using ExchangeRateProvider.Application.ExchangeRateProviders.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class ProviderApiAccountController(
    IProviderApiAccountQueryRepository providerapiaccountQuery,
    IProviderApiAccountCommandRepository providerapiaccountCommand,
    IProviderQueryRepository providerQueryRepository,
    EncryptionConfiguration encryptionConfiguration,
    INotifier notifier,
    ILogger<ExchangeRateProviderApiAccount> logger) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> List(
        string owner,
        ProviderType type,
        string published,
        int index = 1,
        RequestPagingSize size = RequestPagingSize.Ten,
        CancellationToken cancellationToken = default)
    {
        var filter = new ProviderApiAccountPaginatedFilterRequest
        {
            Owner = owner,
            ProviderType = type,
            Published = string.IsNullOrEmpty(published) ? null : int.Parse(published) == 1,
        };

        var paging = new RequestPaging
        {
            Index = index,
            Size = size
        };

        var request = new PaginationRequest<ProviderApiAccountPaginatedFilterRequest>
        {
            Filter = filter,
            Paging = paging
        };

        var response = await providerapiaccountQuery.GetProviderApiAccountsWithPaginationAndFilterAsync(request, cancellationToken);

        var model = new GetProviderApiAccountListModel
        {
            PaginationResponse = response,
            Owner = owner,
            Published = string.IsNullOrEmpty(published) ? null : published.ToString(),
            SizeOptions = GetPageSizeOptions((int)size),
            ProviderTypesOptions = EnumHelper.GetEnumTypeOptions<ProviderType>(),
            ProtocolTypesOptions = EnumHelper.GetEnumTypeOptions<ProtocolType>()
        };

        return View(model);
    }

    public IActionResult Add()
    {
        CreateProviderApiAccountModel model = new()
        {
            ProviderTypesOptions = EnumHelper.GetEnumTypeOptions<ProviderType>(false),
            ProtocolTypesOptions = EnumHelper.GetEnumTypeOptions<ProtocolType>(false)
        };
        return View(new ResponseWrapper<CreateProviderApiAccountModel>(model));
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateProviderApiAccountModel>> Add([FromForm] ResponseWrapper<CreateProviderApiAccountModel> model, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateProviderApiAccountRequest
            {
                CreatedById = GetCurrentUserId(),
                Owner = model.Response.Owner,
                Credentials = model.Response.Credentials.SetCredentials(encryptionConfiguration.EncryptionKey),
                Description = model.Response.Description,
                ProtocolType = model.Response.ProtocolType,
                ProviderType = model.Response.Type,
                Published = model.Response.Published
            };
            var result = await ProviderApiAccountHandlers.CreateAsync(request, providerapiaccountCommand, providerQueryRepository, notifier, logger);

            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "ProviderApiAccount"));
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
            var result = await ProviderApiAccountHandlers.GetByIdAsync(id, providerapiaccountQuery, logger);
            CreateProviderApiAccountModel model = new()
            {
                Owner = result.Owner,
                Published = result.Published,
                Type = result.Type,
                ProtocolType = result.ProtocolType,
                Credentials = result.EncryptedCredentials.GetCredentials(encryptionConfiguration.EncryptionKey),
                Description = result.Description,
                ProviderTypesOptions = EnumHelper.GetEnumTypeOptions(result.Type),
                ProtocolTypesOptions = EnumHelper.GetEnumTypeOptions(result.ProtocolType)
            };
            return View(new ResponseWrapper<CreateProviderApiAccountModel>(model));
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
    public async Task<ResponseWrapper<CreateProviderApiAccountModel>> Edit([FromRoute] int id, ResponseWrapper<CreateProviderApiAccountModel> model, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateProviderApiAccountRequest()
            {
                CreatedById = GetCurrentUserId(),
                Owner = model.Response.Owner,
                Credentials = model.Response.Credentials.SetCredentials(encryptionConfiguration.EncryptionKey),
                Description = model.Response.Description,
                ProtocolType = model.Response.ProtocolType,
                ProviderType = model.Response.Type,
                Published = model.Response.Published
            };
            var result = await ProviderApiAccountHandlers.UpdateAsync(request, id, providerapiaccountCommand, providerapiaccountQuery, providerQueryRepository, notifier, logger);
            
            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "ProviderApiAccount"));
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
    public IActionResult GetProtocolTypes()
    {
        var protocols = EnumHelper.GetEnumTypeOptions<ProtocolType>(false);
        return Json(protocols);
    }
}

