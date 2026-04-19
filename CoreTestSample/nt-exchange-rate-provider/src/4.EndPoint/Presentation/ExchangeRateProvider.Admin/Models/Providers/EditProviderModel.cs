using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Admin.Models.Providers;

public class EditProviderModel
{
    public int Id { get; set; }
    public ProviderType ProviderType { get; set; }

    [Required]
    public string Name { get; set; }
    public bool Published { get; set; }

    public List<string> Markets { get; set; }

    public List<string> SelectedMarkets { get; set; }

    public IEnumerable<SelectListItem> ProviderTypesOptions { get; set; } = [];
}