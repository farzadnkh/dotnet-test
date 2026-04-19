namespace ExchangeRateProvider.Api.Models.Accounts;

public class LogoutViewModel
{
    public string LogoutId { get; set; }
    public bool ShowLogoutPrompt { get; set; } = true;
    public string PostLogoutRedirectUri { get; set; }
}
