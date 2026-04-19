using ExchangeRateProvider.Domain.Users.Entities;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.Users;

public interface IUserQueryRepository : IBaseQueryRepository<User, int>
{
}
