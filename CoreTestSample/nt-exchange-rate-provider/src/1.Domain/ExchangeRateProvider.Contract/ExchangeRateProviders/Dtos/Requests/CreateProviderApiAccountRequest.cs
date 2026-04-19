using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Requests;

public class CreateProviderApiAccountRequest
{
    public ProviderType ProviderType { get; set; }
    public ProtocolType ProtocolType { get; set; }
    public string Owner { get; set; }
    public byte[] Credentials { get; set; }
    public bool Published { get; set; }
    public string Description { get; set; }
    public int CreatedById { get; set; }
}
