namespace NT.SDK.ExchangeRateProvider.Resources;

internal static class Endpoints
{
    /// <summary>
    /// API endpoint for generating a token.
    /// </summary>
    internal const string Get_Token = "/api/v1/app/accounts/login";

    /// <summary>
    /// API endpoint for retrieving the latest prices.
    /// </summary>
    internal const string Get_Rates_Api = "api/v1/app/pairs/latest/price";

    /// <summary>
    /// Socket endpoint for retrieving the latest prices.
    /// </summary>
    internal const string Get_Rates_Socket = "/wss/price";
}
