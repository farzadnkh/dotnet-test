using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;

public class ProviderApiAccountPaginatedFilterRequest
{
    public ProviderType ProviderType { get; set; }
    public string Owner { get; set; }
    public bool? Published { get; set; }
}
