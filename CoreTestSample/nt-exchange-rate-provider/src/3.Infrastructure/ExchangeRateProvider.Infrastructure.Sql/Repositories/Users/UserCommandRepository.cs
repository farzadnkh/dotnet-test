using Duende.IdentityServer.EntityFramework.DbContexts;
using ExchangeRateProvider.Contract.Users;
using ExchangeRateProvider.Domain.Users.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Users;

public class UserCommandRepository(
    IUnitOfWork<int> unitOfWork,
    PersistedGrantDbContext persistedGrantDbContext,
    ILogger<UserCommandRepository> logger) : BaseCommandRepository<User, int, int>(unitOfWork, logger), IUserCommandRepository
{
    public async Task RemoveUserPersistedGrantAsync(string clientId, CancellationToken cancellationToken)
    {
        var persistedGrants = await persistedGrantDbContext.PersistedGrants.Where(item => item.ClientId.Equals(clientId))
                                                                           .ToListAsync(cancellationToken);
        if(persistedGrants.Count > 0)
        {
            persistedGrantDbContext.PersistedGrants.RemoveRange(persistedGrants);
            await persistedGrantDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
