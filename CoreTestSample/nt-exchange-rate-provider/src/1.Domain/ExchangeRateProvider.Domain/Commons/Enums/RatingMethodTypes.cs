using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Domain.Commons.Enums;

public enum RatingMethod : byte
{
    None = 0,

    [Display(Name = "Automatic Rating")]
    Automatic = 1,

    [Display(Name = "Manual Rating")]
    Manual = 2
}
