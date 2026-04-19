using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

namespace ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;

public class ProviderApiAccountResponse
{
    public ProviderApiAccountResponse(
        int id,
        string owner,
        ProviderType type,
        bool published,
        ProtocolType protocolType,
        string description,
        string createdAt)
    {
        Id = id;
        Owner = owner;
        Type = type;
        Published = published;
        Description = description;
        ProtocolType = protocolType;
        CreatedAt = createdAt;
    }

    public ProviderApiAccountResponse()
    {
    }


    public int Id { get; private set; }
    public string Owner { get; private set; }
    public ProviderType Type { get; private set; }
    public ProtocolType ProtocolType { get; private set; }
    public bool Published { get; private set; }
    public string Description { get; private set; }
    public byte[] EncryptedCredentials { get; set; } = null;
    public ProviderApiAccountCredentials DecryptedCredentials { get; set; } = new();
    public string CreatedAt { get; set; }
}
