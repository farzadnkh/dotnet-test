using ExchangeRateProvider.Contract.Markets.Dtos.Responses;

namespace ExchangeRateProvider.Admin.Models.MarketTradingPairs;

public class GetMarketTradingPairListModel : BaseListViewModel<MarketTradingPairResponse>
{
    public int? MarketId { get; set; }
    public int? CurrencyId { get; set; }

    public IEnumerable<SelectListItem> CurrencyIdOptions { get; set; } = [];
    public IEnumerable<SelectListItem> MarketIdOptions { get; set; } = [];
}
