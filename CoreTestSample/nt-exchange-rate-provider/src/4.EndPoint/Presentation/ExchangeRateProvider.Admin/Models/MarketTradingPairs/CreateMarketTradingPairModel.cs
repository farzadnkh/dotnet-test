using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;

namespace ExchangeRateProvider.Admin.Models.MarketTradingPairs;

public class CreateMarketTradingPairModel
{
    public string MarketId { get; set; }
    public string CurrencyId { get; set; }
    public List<string> ExchangeRateProviderIds { get; set; }
    public RatingMethod RatingMethod { get; set; }
    public bool Published { get; set; }
    public string Description { get; set; }

    public string ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = [];

    public IEnumerable<SelectListItem> ExchangeRateProviderIdsOptions { get; set; } = [];
    public IEnumerable<SelectListItem> CurrencyIdOptions { get; set; } = [];
    public IEnumerable<SelectListItem> MarketIdOptions { get; set; } = [];
    public IEnumerable<SelectListItem> RatingMethodOptions { get; set; } = [];
    public SpreadOptions SpreadOptions { get; set; } = new();
}
