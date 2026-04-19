using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Admin.Models.Markets;

public class CreateMarketModel
{
    public string MarketCurrencyId { get; set; }
    public MarketCalculationTerm? MarketCalculationTerms { get; set; }
    public RatingMethod RatingMethod { get; set; }
    public List<string> ExchangeRateProviderIds { get; set; }
    public List<string> ExchangeRateProviderNames { get; set; }

    public bool IsDefault { get; set; }
    public bool Published { get; set; }
    public bool CreateAllFiats { get; set; }
    public bool CreateAllCryptos { get; set; }

    public string ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();

    public IEnumerable<SelectListItem> MarketCurrencyIdsOptions { get; set; } = [];
    public IEnumerable<SelectListItem> ExchangeRateProviderIdsOptions { get; set; } = [];
    public IEnumerable<SelectListItem> CalculationOptions { get; set; } = [];
    public IEnumerable<SelectListItem> RatingMethodOptions { get; set; } = [];
    public SpreadOptions SpreadOptions { get; set; } = new SpreadOptions();
}
