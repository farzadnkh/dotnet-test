using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Dtos.Responses;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Consumers.Entities;
using NT.DDD.Base.Paginations.Requests;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Repository.Contract.Queries;

namespace ExchangeRateProvider.Contract.Consumers;

public interface IConsumerQueryRepository : IBaseQueryRepository<Consumer, int>
{
    Task<IPaginationResponse<ConsumerResponse>> GetConsumersWithPaginationAndFilterAsync(
        IPaginationRequest<ConsumerPaginatedFilterRequest> request,
        CancellationToken cancellationToken = default);

    Task<IPaginationResponse<ConsumerConfigurationResponse>> GetConsumersConfigurationWithPaginationAndFilterAsync(
        IPaginationRequest<ConsumerConfigurationPaginatedFilterRequest> request,
        CancellationToken cancellationToken = default);

    Task<Consumer> GetByIdWithAllIncludesAsync(int id, CancellationToken cancellationToken = default);
    Task<Consumer> GetConfigurationByConsumerIdWithAllIncludesAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsExistApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    Task<bool> IsExistProjectNameAsync(string projectName, CancellationToken cancellationToken = default);
    
    Task<Consumer> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    SpreadOptions GetSpreadOptions(ICollection<ConsumerPair> pairs = null, ICollection<ConsumerMarket> markets = null);

    Task<IEnumerable<Consumer>> GetAllWithAllIncludesAsync(CancellationToken cancellationToken);
    
    Task<Consumer> GetConsumerWithAllData(int consumerId, CancellationToken cancellationToken);
    Task<IEnumerable<ConsumerPair>> GetConsumerAllActivePairs(int consumerId, CancellationToken cancellationToken);

    Task<Consumer> GetReadOnlyConfigurationByConsumerIdWithAllIncludesAsync(int id,
        CancellationToken cancellationToken = default);
}
