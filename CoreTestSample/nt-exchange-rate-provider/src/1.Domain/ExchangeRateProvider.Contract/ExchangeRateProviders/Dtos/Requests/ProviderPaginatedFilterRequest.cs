using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;

public class ProviderPaginatedFilterRequest
{
    public string Name { get; set; }
    public ProviderType ProviderType { get; set; }
    public bool? Published { get; set; }
}
