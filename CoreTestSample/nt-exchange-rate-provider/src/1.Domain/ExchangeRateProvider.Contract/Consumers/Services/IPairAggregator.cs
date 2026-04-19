using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Contract.Consumers.Services;

public interface IPairAggregator
{
    public Task<PairExchangeRateDto> GetPairExchangeRate(int consumerId, TradingPairSnapshot tradingPair); 
}