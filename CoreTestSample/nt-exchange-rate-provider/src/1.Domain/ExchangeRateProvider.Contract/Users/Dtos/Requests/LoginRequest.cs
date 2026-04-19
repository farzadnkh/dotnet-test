namespace ExchangeRateProvider.Contract.Users.Dtos.Requests;

public class LoginRequest
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }
}
