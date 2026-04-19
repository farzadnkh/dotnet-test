using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Contract.Consumers.Services;

public interface IExchangeRateSnapshotMemory : ISingletonLifetime
{
    public void SetTradingPairSnapShot(int consumerId, int tradingPairId, TradingPairSnapshot snapshot);
    public void SetPairExchangeRateSnapShot(int consumerId, int tradingPairId, PairExchangeRateDto exchangeRatePair);

    (bool, TradingPairSnapshot?) TryToGetTradingPairSnapShot(int consumerId, int tradingPairId);
    IEnumerable<PairExchangeRateDto> GetAllForConsumer(int consumerId);

    (bool, IEnumerable<TradingPairSnapshot>) TryToGetConsumerAllPairs(int consumerId);

    (bool, SpreadOptions) TryToGetSpreadOptionsMemory(int consumerId, int tradingPairId);

    void UpdateSpreadOptionsMemory(int consumerId, int tradingPairId, SpreadOptions snapshot);

    void RemoveExchangeRateSnapShot(int consumerId, int tradingPairId);
    void RemoveAllExchangeRateSnapShotForConsumer(int consumerId);
    void RemoveAllTradingPairsSnapShotForConsumer(int consumerId);
    void RemoveTradingPairSnapShot(int consumerId, int tradingPairId);

    void UpdateConsumerPairsSnapshot(int consumerId, IEnumerable<TradingPairSnapshot> snapshot);

    Dictionary<int, List<TradingPairSnapshot>> GetAllConsumersTradingPairs();
    
    void RemoveAllMemoryForConsumer(int consumerId);
    void RemoveAllSpreadOptionsForConsumer(int consumerId);

    void SetLastExposedPrice(int consumerId, int tradingPairId, decimal price);
    decimal? TryGetLastExposedPrice(int consumerId, int tradingPairId);
    void RemoveAllLasExposedPrices(int consumerId);

    void SetConsumer(int consumerId, Consumer consumer);
    Consumer TryGetConsumer(int consumerId);
    void RemoveConsumer(int consumerId);

    void ResetAllCache();
}
