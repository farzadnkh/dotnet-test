using NT.IDP.BaseIdentity.Entities;

namespace ExchangeRateProvider.Domain.Users.Entities
{
    public class User : BaseUser<int>
    {
        public bool IsActive { get; set; } = true;
    }
}
