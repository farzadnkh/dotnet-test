using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Users.Dtos.Requests;
using ExchangeRateProvider.Contract.Users.Dtos.Responses;
using ExchangeRateProvider.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace ExchangeRateProvider.Application.Users.Handlers.Admin;

public static class UserHandlers
{
    public static async Task<ResponseWrapper<CreateUserResponse>> CreateUserConsumerAsync(
            [FromBody] CreateUserModel request,
            UserManager<User> userManager,
            ILogger<User> logger)
    {
        var result = new ResponseWrapper<CreateUserResponse>();

        await ValidateModel(request.Email, request.Username, userManager, logger);

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            NormalizedEmail = request.Email.Normalize(),
            NormalizedUserName = request.Username.Normalize(),
            EmailConfirmed = true,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            IsActive = request.IsActive
            
        };

        var identityUserResult = await userManager.CreateAsync(user);

        if (!identityUserResult.Succeeded)
        {
            result.IsSuccess = false;
            result.Errors.Add("Could not Create New User");
            return result;
        }

        logger.LogInformation("User created successfully for consumer: {UserName}", user.UserName);

        result.Response = new CreateUserResponse { Id = user.Id, IsSuccess = true, Message = "User created successfully." };
        return result;
    }

    public static async Task<EditUserModel> GetUserById(int id, UserManager<User> userManager)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user != null)
            return new EditUserModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.UserName,
                IsActive = user.IsActive
            };

        return null;
    }

    public static async Task<ResponseWrapper<EditUserModel>> UpdateUser(EditUserModel model, UserManager<User> userManager, ILogger<User> logger)
    {
        var result = new ResponseWrapper<EditUserModel>();

        var user = await userManager.FindByIdAsync(model.Id.ToString());
        if (user == null)
        {
            logger.LogInformation("User with Id: {id} not Found.", model.Id);
            result.Response = new();
            result.IsSuccess = false;
            result.AddError("User Not Found.");
            return result;
        }
        await ValidateModel(model.Email, model.Username, user, userManager, logger);

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Username;
        user.IsActive = model.IsActive;
        var identityUserResult = await userManager.UpdateAsync(user);

        if (!identityUserResult.Succeeded)
        {
            logger.LogInformation("Update user With id: {id} was not successful", model.Id);
            result.Response = new();
            result.IsSuccess = false;
            result.AddError("Can not Update User.");
            return result;
        }

        logger.LogInformation("User Updated successfully for consumer: {UserName}", user.UserName);

        result.Response = new EditUserModel { Id = user.Id };
        return result;
    }

    public static async Task<bool> DeactivateAsync(
            [FromRoute] int id,
            UserManager<User> userManager,
            ILogger<User> logger)
    {
        try
        {
            logger.LogInformation("Attempting to deactivate consumer with ID: {Id}", id);

            if (id <= 0)
            {
                logger.LogWarning("Invalid consumer ID provided for deactivation: {Id}", id);
                throw ApplicationBadRequestException.Create("ID is required and must be a positive integer.");
            }

            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                logger.LogWarning("Consumer with ID {Id} not found for deactivation.", id);
                throw ApplicationNotFoundException.Create($"Consumer with ID: {id} not found.");
            }

            user.IsActive = false;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                logger.LogInformation("Consumer deactivated successfully with ID {Id}.", id);
            }
            else
            {
                logger.LogWarning("Failed to deactivate consumer with ID {Id}. Update operation returned false.", id);
                throw ApplicationBadRequestException.Create("Failed to deactivate consumer. The update operation was not successful.");
            }

            return result.Succeeded;
        }
        catch (ApplicationBadRequestException ex)
        {
            logger.LogWarning(ex, "Bad request while deactivating consumer with ID {Id}: {ErrorMessage}", id, ex.Message);
            throw;
        }
        catch (ApplicationNotFoundException ex)
        {
            logger.LogWarning(ex, "Consumer not found while deactivating with ID {Id}: {ErrorMessage}", id, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deactivating consumer with ID {Id}.", id);
            throw ApplicationBadRequestException.Create("An unexpected error occurred while deactivating the consumer.", (int)HttpStatusCode.InternalServerError);
        }
    }

    public static async Task ValidateModel(
            string email,
            string username,
            UserManager<User> userManager,
            ILogger<User> logger)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw ApplicationBadRequestException.Create("Email is required.");
        if (string.IsNullOrWhiteSpace(username))
            throw ApplicationBadRequestException.Create("User Name is required.");

        var isEmailExist = await userManager.FindByEmailAsync(email);
        if (isEmailExist is not null)
        {
            logger.LogWarning("Consumer creation failed: Duplicate email '{Email}' is not acceptable.", email);
            throw ApplicationBadRequestException.Create("Duplicate Email is not acceptable.");
        }

        var isExistUserName = await userManager.FindByNameAsync(username);
        if (isExistUserName is not null)
        {
            logger.LogWarning("Consumer creation failed: Duplicate username '{UserName}' is not acceptable.", username);
            throw ApplicationBadRequestException.Create("Duplicate Username is not acceptable.");
        }
    }

    public static async Task ValidateModel(
        string email,
        string username,
        User currentUser,
        UserManager<User> userManager,
        ILogger<User> logger)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw ApplicationBadRequestException.Create("Email is required.");
        if (string.IsNullOrWhiteSpace(username))
            throw ApplicationBadRequestException.Create("User Name is required.");

        if(currentUser.Email != email)
        {
            var isEmailExist = await userManager.FindByEmailAsync(email);
            if (isEmailExist is not null)
            {
                logger.LogWarning("Consumer creation failed: Duplicate email '{Email}' is not acceptable.", email);
                throw ApplicationBadRequestException.Create("Duplicate Email is not acceptable.");
            }
        }

        if(currentUser.UserName != username)
        {
            var isExistUserName = await userManager.FindByNameAsync(username);
            if (isExistUserName is not null)
            {
                logger.LogWarning("Consumer creation failed: Duplicate username '{UserName}' is not acceptable.", username);
                throw ApplicationBadRequestException.Create("Duplicate Username is not acceptable.");
            }
        }
    }
}