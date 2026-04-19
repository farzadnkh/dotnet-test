using NT.SDK.RestClient.Models;

namespace NT.SDK.ExchangeRateProvider.Models.Options;

public class ExchangeRateProviderOptions
{
    public IReadableConfiguration Configuration { get; set; }

    /// <summary>
    /// BasePath is optional. If you are providing the BasePath in Vault, 
    /// there is no need to set this property.
    /// </summary>
    public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Cache prefix used to separate saved data in Redis related to the ExchangeRateProvider.
    /// </summary>
    public string CachePrefix { get; set; } = "ExRP:";

    /// <summary>
    /// After receiving your credentials, you will have the token's expiration time. 
    /// Set that value (in seconds) here.
    /// </summary>
    public int TokenExpireInSec { get; set; } = 3600;
}
