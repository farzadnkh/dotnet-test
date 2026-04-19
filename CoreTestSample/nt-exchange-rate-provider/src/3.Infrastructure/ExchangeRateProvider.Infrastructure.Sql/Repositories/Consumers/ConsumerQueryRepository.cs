using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Domain.Consumers.Entities;
using System.Threading.Tasks;

namespace ExchangeRateProvider.Infrastructure.Sql.Repositories.Consumers;

public class ConsumerCommandRepository(
    IUnitOfWork<int> unitOfWork,
    ILogger<ConsumerCommandRepository> logger) : BaseCommandRepository<Consumer, int>(unitOfWork, logger), IConsumerCommandRepository
{
    public async Task DeleteAllConfigurations(ICollection<ConsumerMarket> consumerMarkets, ICollection<ConsumerPair> consumerPairs, ICollection<ConsumerProvider> consumerProviders)
    {
		try
		{
            _unitOfWork.DbContext.Set<ConsumerMarket>().RemoveRange(consumerMarkets);
            _unitOfWork.DbContext.Set<ConsumerPair>().RemoveRange(consumerPairs);
            _unitOfWork.DbContext.Set<ConsumerProvider>().RemoveRange(consumerProviders);

            await SaveChangesAsync(default);
        }
		catch (Exception ex)
		{
            _logger.LogCritical("Removing Configurations failed. Error:{ex}", ex);
		}
    }
}
