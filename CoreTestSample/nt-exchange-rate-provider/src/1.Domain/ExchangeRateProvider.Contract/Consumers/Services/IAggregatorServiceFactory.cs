using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Domain.Commons.Dtos;

namespace ExchangeRateProvider.Contract.Consumers.Services;

public interface IAggregatorServiceFactory
{
    IAsyncEnumerable<PairExchangeRateDto> GetPairExchangeRatesForConsumerAllPairsAsync(int consumerId,
        GetExchangeRateForAllPairsWithFilter filter, bool shouldApplyNewFilter, CancellationToken ct);
}
