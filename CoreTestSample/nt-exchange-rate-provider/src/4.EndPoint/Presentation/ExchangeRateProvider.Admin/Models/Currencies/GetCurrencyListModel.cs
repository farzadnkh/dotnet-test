using ExchangeRateProvider.Contract.Currencies.Dtos.Responses;
using ExchangeRateProvider.Domain.Currencies.Enums;

namespace ExchangeRateProvider.Admin.Models.Currencies
{
    public class GetCurrencyListModel : BaseListViewModel<CurrencyResponse>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public CurrencyType? Type { get; set; }
        public IEnumerable<SelectListItem> CurrencyTypesOptions { get; set; } = [];
    }
}
