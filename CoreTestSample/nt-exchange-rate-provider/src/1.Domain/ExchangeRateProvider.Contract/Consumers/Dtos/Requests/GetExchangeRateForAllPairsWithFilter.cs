using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.Consumers.Dtos.Requests;

public class GetExchangeRateForAllPairsWithFilter
{
    /// <summary>
    /// Market is basically the Quote Currency, This Filed is not required.
    /// </summary>
    /// <example>USDT</example> 
    public List<string> Markets { get; set; } = [];
 
    /// <summary>
    /// Pairs is basically the BaseCurrency, This Filed is not required.
    /// </summary>
    /// <example>BTC</example>
    public List<string> Pairs { get; set; } = [];
 
    /// <summary>
    /// ProviderTypes, This Filed is not required.
    /// </summary>
    /// <example>BTC</example>
    public ProviderType ProviderTypes { get; set; } = ProviderType.None;
}