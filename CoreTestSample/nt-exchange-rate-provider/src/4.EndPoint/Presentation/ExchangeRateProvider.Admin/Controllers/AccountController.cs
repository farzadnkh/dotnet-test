using ExchangeRateProvider.Contract.Commons.Options;

namespace ExchangeRateProvider.Admin.Controllers;

public class AccountController(BaseUri baseUri) : Controller
{
    [Authorize(AuthenticationSchemes = "oidc")]
    public async Task<IActionResult> Logout()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        }, "Cookies", "oidc");
    }
}