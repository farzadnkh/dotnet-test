using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;

public class ProviderResponse
{
    public ProviderResponse(
        int id,
        string name,
        ProviderType type,
        bool published,
        List<string> selectedMarkets)
    {
        Id = id;
        Name = name;
        Type = type;
        Published = published;
        SelectedMarkets = selectedMarkets;
    }

    public ProviderResponse()
    {
    }


    public int Id { get; private set; }
    public string Name { get; private set; }
    public ProviderType Type { get; private set; }
    public bool Published { get; private set; }
    public List<string> SelectedMarkets { get; private set; }
}
