using NT.SDK.RestClient.Models;

namespace NT.SDK.ExchangeRateProvider.Models.Requests;

public class GetTokenRequest : IRequestBody
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId, nameof(ClientId));
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientSecret, nameof(ClientId));
        ArgumentException.ThrowIfNullOrWhiteSpace(Scopes, nameof(ClientId));
    }
}
