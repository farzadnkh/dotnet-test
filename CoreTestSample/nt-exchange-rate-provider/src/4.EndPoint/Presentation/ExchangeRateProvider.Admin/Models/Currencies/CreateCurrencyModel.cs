using ExchangeRateProvider.Domain.Currencies.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Admin.Models.Currencies;

public class CreateCurrencyModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    [Required]
    public CurrencyType Type { get; set; }
    public int CreatedById { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "DecimalPrecision cannot be negative.")]
    public int? DecimalPrecision { get; set; }
    public bool Published { get; set; }

    [Required]
    public string Symbol { get; set; }
    public List<int> SelectedMarketIds { get; set; } = [];

    public IEnumerable<SelectListItem> MarketOptions { get; set; } = [];
    public IEnumerable<SelectListItem> CurrencyTypesOptions { get; set; } = [];
}
