using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Currencies.Enums;

namespace ExchangeRateProvider.Admin.Controllers
{
    public class BaseController : Controller
    {
        public bool? ConvertToBoolean(string isActive)
        {
            return isActive switch
            {
                "1" => true,
                "0" => false,
                _ => null,
            };
        }

        public int GetCurrentUserId()
        {
            var userIdStr = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier")).Value;
            var result = int.TryParse(userIdStr, out var userId);
            if (result)
                return userId;
            return default;
        }

        public string CurrentUserName()
            => HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("name")).Value.ToString().Split('@')[0];

        public SelectList GetPageSizeOptions(int selectedSize)
        {
            var sizes = new[] { 5, 10, 20, 50 };
            return new SelectList(sizes, selectedSize);
        }

        public static List<SelectListItem> GetPublishOptions(bool? selected)
        {
            return new List<SelectListItem>
            {
                new() { Value = "", Text = "All", Selected = !selected.HasValue },
                new() { Value = "true", Text = "Published", Selected = selected.HasValue && selected.Value },
                new() { Value = "false", Text = "Unpublished", Selected = selected.HasValue && !selected.Value }
            };
        }

        public void ValidateSpreadOption(SpreadOptions spreadOptions)
        {
            if(spreadOptions is not null && spreadOptions.SpreadEnabled)
            {
                if (spreadOptions.UpperLimitPercentage.HasValue && spreadOptions.UpperLimitPercentage.Value < 0) throw ApplicationBadRequestException.Create("Upper limit must be greater than or equal to 0.");
                if (spreadOptions.LowerLimitPercentage.HasValue && spreadOptions.LowerLimitPercentage.Value < 0) throw ApplicationBadRequestException.Create("Lower limit must be greater than or equal to 0.");
                if ((spreadOptions.UpperLimitPercentage.HasValue && spreadOptions.LowerLimitPercentage.HasValue) 
                    && (spreadOptions.UpperLimitPercentage.Value < spreadOptions.LowerLimitPercentage.Value)) 
                    throw ApplicationBadRequestException.Create("Upper limit must be greater than or equal to lower limit.");
            }
        }
    }
}
