using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

public enum ProviderType : byte
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Crypto Compare")]
    CryptoCompare = 3,

    [Display(Name = "Xe")]
    XE = 4
}
