namespace NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Options;

public class ExrpOptions
{
    /// <summary>
    /// Cache prefix used to separate saved data in Redis related to the ExchangeRateProvider.
    /// </summary>
    public string CachePrefix { get; set; } = "ExRP";

    /// <summary>
    /// After receiving your credentials, you will have the token's expiration time. 
    /// Set that value (in seconds) here.
    /// </summary>
    public int TokenExpireInSec { get; set; } = 3600;
}
