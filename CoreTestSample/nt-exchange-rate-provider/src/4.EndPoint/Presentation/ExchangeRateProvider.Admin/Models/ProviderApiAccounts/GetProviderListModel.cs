using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Admin.Models.ProviderApiAccounts;

public class GetProviderApiAccountListModel : BaseListViewModel<ProviderApiAccountResponse>
{
    public string Owner { get; set; }
    public ProviderType Type { get; set; } = ProviderType.None;
    public ProtocolType ProtocolType { get; set; } = ProtocolType.None;
    public ProviderApiAccountCredentials Credentials { get; set; }
    public IEnumerable<SelectListItem> ProviderTypesOptions { get; set; } = [];
    public IEnumerable<SelectListItem> ProtocolTypesOptions { get; set; } = [];
}
