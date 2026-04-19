namespace ExchangeRateProvider.Contract.Commons.Options;

public class BearerAuthenticateConfiguration
{
    public bool RequireHttpsMetadata { get; set; }
    public string Authority { get; set; }
    public TokenValidationParameters TokenValidationParameters { get; set; }
}

public class TokenValidationParameters
{
    public string ValidateIssuer { get; set; }
    public string ValidateAudience { get; set; }
}
