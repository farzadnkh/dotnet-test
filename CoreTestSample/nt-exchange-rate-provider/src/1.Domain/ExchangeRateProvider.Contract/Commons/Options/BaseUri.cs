namespace ExchangeRateProvider.Contract.Commons.Options;

public class BaseUri
{
    public string IdpBaseUri { get; set; } = "https://localhost:5038";
    public string SelfUri { get; set; } = "https://localhost:5038";
    public string AdminPanelUri { get; set; } = "https://localhost:7065/";
    public string CryptoCompareProviderBaseUri { get; set; }
    public string CryptoCompareProviderApiBaseUri { get; set; } = "https://data-api.coindesk.com";
    public string XeCdApiBaseUri { get; set; } = "https://xecdapi.xe.com/";
}
