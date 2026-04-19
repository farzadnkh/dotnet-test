using System.ComponentModel.DataAnnotations;

namespace NT.SDK.ExchangeRateProvider.Models.Enums;

public enum ProviderType : byte
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Crypto Compare")]
    CryptoCompare = 3,

    [Display(Name = "Xe")]
    XE = 4
}
