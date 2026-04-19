using ExchangeRateProvider.Contract.Commons.Options;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly BaseUri _baseUri;

    public HomeController(ILogger<HomeController> logger, BaseUri baseUri)
    {
        _logger = logger;
        _baseUri = baseUri;
    }

    public IActionResult Index()
    {
        ViewData["BaseUrl"] = _baseUri.IdpBaseUri;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
