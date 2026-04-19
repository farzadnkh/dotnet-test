using ExchangeRateProvider.Contract.Markets.Dtos.Responses;
using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Admin.Models.Markets
{
    public class GetMarketListModel : BaseListViewModel<MarketResponse>
    {
        public int? CurrencyId { get; set; }
        public MarketCalculationTerm? Term { get; set; }

        public IEnumerable<SelectListItem> CurrencyIdOptions { get; set; } = [];
        public IEnumerable<SelectListItem> CalculationOptions { get; set; } = [];
    }
}
