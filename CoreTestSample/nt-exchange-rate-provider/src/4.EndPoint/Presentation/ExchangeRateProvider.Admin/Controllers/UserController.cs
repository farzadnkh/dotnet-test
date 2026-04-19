using Duende.IdentityServer.Extensions;
using ExchangeRateProvider.Admin.Models.Users;
using ExchangeRateProvider.Application.Users.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Users.Dtos.Requests;
using ExchangeRateProvider.Contract.Users.Dtos.Responses;
using Microsoft.OpenApi.Validations;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class UserController(UserManager<User> userManager, ILogger<User> logger) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> List(
        string email,
        string username,
        string isActive,
        int index = 1,
        RequestPagingSize size = RequestPagingSize.Ten,
        CancellationToken cancellationToken = default)
    {
        var filter = new UserPaginatedFilterRequest
        {
            Username = username,
            Email = email,
            IsActive = string.IsNullOrEmpty(isActive) ? null : int.Parse(isActive) == 1,
        };

        var paging = new RequestPaging
        {
            Index = index,
            Size = size
        };

        var request = new PaginationRequest<UserPaginatedFilterRequest>
        {
            Filter = filter,
            Paging = paging
        };

        var query = userManager.Users.Where(u => !u.IsDeleted).OrderByDescending(u => u.Id).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Username))
            query = query.Where(c => c.UserName.Contains(filter.Username));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(c => c.Email.Contains(filter.Email));

        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        List<UserResponse> users = await query
            .ToPagedQuery((int)request.Paging.Size, request.Paging.Index)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                CreatedAt = u.CreatedOnUtc.ToString("MM/dd/yyyy"),
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive
            })
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} users.", totalCount);

        var paginationResult = new PaginationResponse<UserResponse>(
            [.. users.Select(c => new UserResponse
            {
                Id = c.Id,
                UserName = c.UserName,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive
            })],
            new BasePaginationResult((int)Math.Ceiling((double)totalCount / (int)request.Paging.Size), request.Paging.Index, (int?)request.Paging.Size)
        );

        var model = new GetUserListModel
        {
            PaginationResponse = paginationResult,
            UserName = username,
            Email = email,
            SizeOptions = GetPageSizeOptions((int)size),
        };

        return View(model);
    }

    public IActionResult Add([FromQuery] string redirectTo)
    {
        var model = new CreateUserModel()
        {
            RedirectTo = redirectTo
        };
        return View("Add", new ResponseWrapper<CreateUserModel>(model));
    }

    [HttpPost]
    public async Task<ResponseWrapper<CreateUserModel>> Add([FromForm] ResponseWrapper<CreateUserModel> model)
    {
        try
        {
            var result = await UserHandlers.CreateUserConsumerAsync(model.Response, userManager, logger);

            if (!string.IsNullOrWhiteSpace(model.Response.RedirectTo))
            {
                model.Data.Add("redirectUrl", Url.Action("AddConsumer", "Consumer", new { selectedUserId = result.Response.Id }));
            }
            else
            {
                model.Data.Add("redirectUrl", Url.Action("List", "User"));
            }

            model.Errors = result.Errors;
            model.IsSuccess = !result.Errors.Any();
            return model;
        }
        catch (ApplicationBadRequestException ex)
        {
            model.Errors = new List<string>() { ex.Message };
            model.IsSuccess = false;

            return model;
        }
        catch (Exception ex)
        {
            model.Errors = new List<string>() { "Something went Wrong" };
            model.IsSuccess = false;

            return model;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await UserHandlers.GetUserById(id, userManager);
        return View(new ResponseWrapper<EditUserModel>(user));
    }

    [HttpPost]
    public async Task<ResponseWrapper<EditUserModel>> Edit([FromForm] ResponseWrapper<EditUserModel> model)
    {
        try
        {
            var result = await UserHandlers.UpdateUser(model.Response, userManager, logger);

            model.Errors = result.Errors;
            model.Data.Add("redirectUrl", Url.Action("List", "User"));
            model.IsSuccess = !result.Errors.Any();

            return model;
        }
        catch (ApplicationBadRequestException ex)
        {
            model.Errors = new List<string>() { ex.Message };
            model.IsSuccess = false;

            return model;
        }
        catch (Exception ex)
        {
            model.Errors = new List<string>() { "Something went Wrong" };
            model.IsSuccess = false;

            return model;
        }
    }

    public async Task<bool> DeActive([FromRoute] int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivating User with ID: {UserId}", id);

        try
        {
            var result = await UserHandlers.DeactivateAsync(id, userManager, logger);
            logger.LogInformation("User {UserId} deactivated successfully: {Result}", id, result);
            return result;
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Bad request while deactivating User {UserId}: {Message}", id, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while deactivating User {UserId}", id);
            return false;
        }
    }
}