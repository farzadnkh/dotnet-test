using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Repository.Contract.Commands;

namespace ExchangeRateProvider.Contract.Users;

public interface IUserCommandRepository : IBaseCommandRepository<User, int, int>
{
    Task RemoveUserPersistedGrantAsync(string clientId, CancellationToken cancellationToken);
}
