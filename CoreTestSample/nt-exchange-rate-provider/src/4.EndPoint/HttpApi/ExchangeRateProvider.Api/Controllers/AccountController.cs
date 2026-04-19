using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using ExchangeRateProvider.Api.Models.Accounts;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Domain.Users.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateProvider.Api.Controllers;

public class AccountController(IIdentityServerInteractionService interaction,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        BaseUri baseUri,
        ILogger<AccountController> logger,
        IEventService events) : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var context = await interaction.GetAuthorizationContextAsync(returnUrl);

            var user = await userManager.Users.FirstOrDefaultAsync(p => p.UserName == model.UserName);

            if (user is not null)
            {
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, true, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    await events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: null));

                    AuthenticationProperties props = new()
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(20)
                    };

                    var isUser = new IdentityServerUser(user.Id.ToString())
                    {
                        DisplayName = user.UserName,
                        AuthenticationTime = DateTime.Now,
                    };

                    await HttpContext.SignInAsync(isUser, props);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return Redirect("~/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            else
            {
                ModelState.AddModelError("UserName", "User not found.");
            }
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AccountController>>();
        try
        {
            logger.LogInformation("Received Logout GET request with logoutId: {LogoutId}", logoutId);

            var vm = await BuildLogoutViewModelAsync(logoutId, logger);

            if (User?.Identity?.IsAuthenticated == true)
            {
                logger.LogInformation("User is authenticated. Signing out from IdentityServer...");

                try
                {
                    await HttpContext.SignOutAsync();
                    logger.LogInformation("SignOutAsync completed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during HttpContext.SignOutAsync");
                }

                try
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                        logger.LogDebug("Deleted cookie: {Cookie}", cookie);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error deleting cookies.");
                }
            }

            if (string.IsNullOrWhiteSpace(vm?.PostLogoutRedirectUri))
            {
                logger.LogWarning("PostLogoutRedirectUri is missing. Redirecting to fallback Login page.");
                return RedirectToAction("Login");
            }

            logger.LogInformation("Redirecting to: {RedirectUri}", vm.PostLogoutRedirectUri);
            return Redirect(vm.PostLogoutRedirectUri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during Logout.");
            return RedirectToAction("Login");
        }
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId, ILogger logger)
    {
        var vm = new LogoutViewModel { LogoutId = logoutId };

        if (User?.Identity?.IsAuthenticated != true)
        {
            logger.LogInformation("User not authenticated - skipping logout prompt.");
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        try
        {
            var context = await interaction.GetLogoutContextAsync(logoutId);
            if (context == null)
            {
                logger.LogWarning("Logout context is null for logoutId: {LogoutId}", logoutId);
            }

            vm.PostLogoutRedirectUri = context?.PostLogoutRedirectUri
                                      ?? HttpContext.Request.Headers["Referer"].ToString()
                                      ?? baseUri.AdminPanelUri;

            logger.LogInformation("PostLogoutRedirectUri resolved to: {Uri}", vm.PostLogoutRedirectUri);

            vm.ShowLogoutPrompt = context?.ShowSignoutPrompt ?? false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching logout context.");
            vm.PostLogoutRedirectUri = baseUri.AdminPanelUri;
        }

        return vm;
    }
}
