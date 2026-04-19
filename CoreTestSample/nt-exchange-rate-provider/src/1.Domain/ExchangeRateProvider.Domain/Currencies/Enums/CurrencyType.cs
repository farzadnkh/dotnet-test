using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Domain.Currencies.Enums;

public enum CurrencyType : byte
{
    [Display(Name = "")]
    None = 0,

    [Display(Name = "Fiat")]
    Fiat = 1,

    [Display(Name = "Crypto")]
    Crypto = 2
}
