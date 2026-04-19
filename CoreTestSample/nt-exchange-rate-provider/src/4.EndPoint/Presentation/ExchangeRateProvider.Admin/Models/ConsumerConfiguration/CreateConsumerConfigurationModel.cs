using ExchangeRateProvider.Domain.Commons;

namespace ExchangeRateProvider.Admin.Models.ConsumerConfiguration;

public class CreateConsumerConfigurationModel
{
    public int ConsumerId { get; set; }
    public List<string> ProviderIds { get; set; }
    public List<string> MarketIds { get; set; }
    public List<string> PairIds { get; set; }
    public bool IsActive { get; set; }

    public IEnumerable<SelectListItem> ProviderOptions { get; set; }
    public IEnumerable<SelectListItem> MarketOptions { get; set; }
    public IEnumerable<SelectListItem> PairOptions { get; set; }
    public IEnumerable<SelectListItem> ConsumerOptions { get; set; }
    public SpreadOptions SpreadOptions { get; set; } = new();
}