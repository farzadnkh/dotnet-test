namespace ExchangeRateProvider.Contract.Users.Dtos.Responses;

public class LoginResponse
{
    public string AccessToken { get; set; }
    public int ExpireInSec { get; set; }
}
