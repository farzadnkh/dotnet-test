using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Domain.Auditing;

namespace ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

public class ProviderBusinessLogic : AuditedEntity<int, User, int>
{
    public int ProviderId { get; set; }
    public Provider ExchangeRateProvider { get; set; }
    public string Name { get; set; }
}