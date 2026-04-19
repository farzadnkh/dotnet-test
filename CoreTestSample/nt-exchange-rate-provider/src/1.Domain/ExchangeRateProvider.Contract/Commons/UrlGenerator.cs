namespace ExchangeRateProvider.Contract.Commons;

public static class UrlGenerator
{
    #region Methods

    public static string GenerateCryptoCompareSocketUrl(string url, string apiKey)
        => $"{url}/v2?api_key={apiKey}";

    public static string GenerateCryptoCompareApiUrl(string apiKey, string market, List<string> instruments)
        => $"/spot/v1/latest/tick?market={market}&instruments={string.Join(',', instruments)}&apply_mapping=false&groups=ID,VALUE,CURRENT_DAY&api_key={apiKey}";

    public static string GenerateXeApiUrl(string market, string instruments, int amount = 1)
        => $"/v1/convert_from/?from={instruments}&to={market}&amount={amount}";

    #endregion

    #region Properties

    #endregion
}