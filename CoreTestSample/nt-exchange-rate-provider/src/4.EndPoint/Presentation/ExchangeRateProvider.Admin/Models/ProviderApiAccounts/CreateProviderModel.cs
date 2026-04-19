using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Admin.Models.ProviderApiAccounts;

public class CreateProviderApiAccountModel
{
    [Required]
    public string Owner { get; set; }

    [Required]
    public ProviderType Type { get; set; }

    [Required]
    public ProtocolType ProtocolType { get; set; }

    public bool Published { get; set; }

    [Required]
    public ProviderApiAccountCredentials Credentials { get; set; }

    public string Description { get; set; }

    public IEnumerable<SelectListItem> ProviderTypesOptions { get; set; }

    public IEnumerable<SelectListItem> ProtocolTypesOptions { get; set; }
}
