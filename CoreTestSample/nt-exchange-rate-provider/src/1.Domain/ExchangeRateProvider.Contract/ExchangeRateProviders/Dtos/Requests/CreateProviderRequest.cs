using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;

public class CreateProviderRequest(string name, ProviderType providerType, bool published, int createdById, List<string> selectedMarkets)
{
    public string Name { get; set; } = name;
    public ProviderType ProviderType { get; set; } = providerType;
    public bool Published { get; set; } = published;
    public int CreatedById { get; set; } = createdById;
    
    public List<string> SelectedMarkets { get; set; } = selectedMarkets;
}
