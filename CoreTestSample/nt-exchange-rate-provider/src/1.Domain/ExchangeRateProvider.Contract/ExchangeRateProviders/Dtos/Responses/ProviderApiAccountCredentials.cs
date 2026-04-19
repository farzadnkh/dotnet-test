namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;

public class ProviderApiAccountCredentials
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string ApiAccount { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
