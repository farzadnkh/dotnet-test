using ExchangeRateProvider.Contract.Users;
using ExchangeRateProvider.Domain.Users.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Users;

public class UserQueryRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<UserQueryRepository> logger) : BaseQueryRepository<User, int>(unitOfWork, logger), IUserQueryRepository
{
}
