using ExchangeRateProvider.Application.Settings.Handlers.Admin;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Contract.Settings.Dtos;

namespace ExchangeRateProvider.Admin.Controllers;

[Authorize(AuthenticationSchemes = "oidc")]
public class SettingController(
    ISettingCommandRepository settingCommandRepository,
    ISettingQueryRepository settingQueryRepository,
    INotifier notifier) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await SettingHandler.GetSetting(settingQueryRepository, cancellationToken);

        return View(new ResponseWrapper<SettingModel>(model));
    }


    [HttpPost]
    public async Task<ResponseWrapper<string>> Save(ResponseWrapper<SettingModel> model, CancellationToken cancellationToken)
    {
        try
        {
            var result = await SettingHandler.Save(model.Response, settingQueryRepository, settingCommandRepository, notifier, cancellationToken);
            return result ? new ResponseWrapper<string>("Successfully updated") : new ResponseWrapper<string>("Failed to update setting");
        }
        catch (ApplicationBadRequestException ex)
        {
            model.IsSuccess = false;
            return new([ex.Message]);
        }
        catch (Exception ex)
        {
            model.IsSuccess = false;
            return new(["Something went Wrong"]);
        }
    }
}