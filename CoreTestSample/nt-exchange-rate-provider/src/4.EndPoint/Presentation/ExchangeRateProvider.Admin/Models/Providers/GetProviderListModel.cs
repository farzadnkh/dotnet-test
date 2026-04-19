using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;

namespace ExchangeRateProvider.Admin.Models.Providers
{
    public class GetProviderListModel : BaseListViewModel<ProviderResponse>
    {
        public string Name { get; set; }
        public string Provider { get; set; }

        public IEnumerable<SelectListItem> ProviderTypesOptions { get; set; } = [];
    }
}
