using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using System.Collections.Concurrent;


namespace ExchangeRateProvider.Application.Consumers.Services;

public class ExchangeRateSnapshotMemory : IExchangeRateSnapshotMemory
{
    // Key: ConsumerId, Value: Dictionary<tradingPairId, Snapshot>
    private ConcurrentDictionary<int, ConcurrentDictionary<int, PairExchangeRateDto>> _exchangeRateSnapshots = new();

    // Key: ConsumerId, Value: Dictionary<tradingPairId, FinalCalculated and Rounded Price>
    private ConcurrentDictionary<int, ConcurrentDictionary<int, decimal>> _lastExposedPrice = new();

    // Key: ConsumerId, Value: TradingPairSnapshot
    private ConcurrentDictionary<int, List<TradingPairSnapshot>> _tradingPairsSnapShot = new();

    private ConcurrentDictionary<(int ConsumerId, int TradingPairId), SpreadOptions> _spreadOptionsMemory =
        new();

    private Dictionary<int, Consumer> _consumersMemory = new();

    public void SetTradingPairSnapShot(int consumerId, int tradingPairId, TradingPairSnapshot snapshot)
    {
        var consumerSnapshots = _tradingPairsSnapShot.GetOrAdd(consumerId, _ => new List<TradingPairSnapshot>());
        consumerSnapshots.Add(snapshot);
    }

    public void SetPairExchangeRateSnapShot(int consumerId, int tradingPairId, PairExchangeRateDto exchangeRatePair)
    {
        var pairs = _exchangeRateSnapshots.GetOrAdd(consumerId,
            _ => new ConcurrentDictionary<int, PairExchangeRateDto>());
        pairs.AddOrUpdate(tradingPairId, exchangeRatePair, (_, _) => exchangeRatePair);
    }

    public (bool, TradingPairSnapshot?) TryToGetTradingPairSnapShot(int consumerId, int tradingPairId)
    {
        _tradingPairsSnapShot.TryGetValue(consumerId, out var consumerSnapshots);
        var snapshot = consumerSnapshots?.FirstOrDefault(i => i.Id == tradingPairId);
        return (snapshot is not null, snapshot);
    }

    public IEnumerable<PairExchangeRateDto> GetAllForConsumer(int consumerId)
    {
        return _exchangeRateSnapshots.TryGetValue(consumerId, out var consumerSnapshots)
            ? consumerSnapshots.Values
            : Enumerable.Empty<PairExchangeRateDto>();
    }

    public (bool, IEnumerable<TradingPairSnapshot>) TryToGetConsumerAllPairs(int consumerId)
    {
        var result = _tradingPairsSnapShot.TryGetValue(consumerId, out var consumerSnapshots);
        return (result, consumerSnapshots);
    }

    public (bool, SpreadOptions) TryToGetSpreadOptionsMemory(int consumerId, int tradingPairId)
    {
        var result = _spreadOptionsMemory.TryGetValue((consumerId, tradingPairId), out var spreadOptions);
        return (result, spreadOptions);
    }

    public void UpdateSpreadOptionsMemory(int consumerId, int tradingPairId, SpreadOptions snapshot)
    {
        _spreadOptionsMemory[(consumerId, tradingPairId)] = snapshot;
    }

    public void RemoveExchangeRateSnapShot(int consumerId, int tradingPairId)
    {
        if (_exchangeRateSnapshots.TryGetValue(consumerId, out var consumerSnapshots))
            consumerSnapshots.TryRemove(tradingPairId, out _);
    }

    public void RemoveAllExchangeRateSnapShotForConsumer(int consumerId)
    {
        _exchangeRateSnapshots.TryRemove(consumerId, out _);
    }

    public void RemoveAllTradingPairsSnapShotForConsumer(int consumerId)
    {
        _tradingPairsSnapShot.TryRemove(consumerId, out _);
    }

    public void RemoveTradingPairSnapShot(int consumerId, int tradingPairId)
    {
        if (_tradingPairsSnapShot.TryGetValue(consumerId, out _))
            _tradingPairsSnapShot.TryRemove(tradingPairId, out _);
    }

    public void UpdateConsumerPairsSnapshot(int consumerId, IEnumerable<TradingPairSnapshot> snapshot)
    {
        var materializedSnapshot = snapshot.ToList();
        _tradingPairsSnapShot.AddOrUpdate(
            consumerId,
            materializedSnapshot,
            (_, _) => materializedSnapshot
        );
    }

    public Dictionary<int, List<TradingPairSnapshot>> GetAllConsumersTradingPairs()
    {
        return _tradingPairsSnapShot.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    public void RemoveAllMemoryForConsumer(int consumerId)
    {
        _exchangeRateSnapshots.TryRemove(consumerId, out _);
        _tradingPairsSnapShot.TryRemove(consumerId, out _);

        RemoveAllSpreadOptionsForConsumer(consumerId);
        RemoveConsumer(consumerId);
    }

    public void RemoveAllSpreadOptionsForConsumer(int consumerId)
    {
        foreach (var key in _spreadOptionsMemory.Keys.Where(k => k.ConsumerId == consumerId).ToList())
        {
            _spreadOptionsMemory.TryRemove(key, out _);
        }
    }

    public void SetLastExposedPrice(int consumerId, int tradingPairId, decimal price)
    {
        var pairs = _lastExposedPrice.GetOrAdd(consumerId,
            _ => new ConcurrentDictionary<int, decimal>());
        pairs.AddOrUpdate(tradingPairId, price, (_, _) => price);
    }

    public decimal? TryGetLastExposedPrice(int consumerId, int tradingPairId)
    {
        return _lastExposedPrice.TryGetValue(consumerId, out var tradingPair)
            ? tradingPair.TryGetValue(tradingPairId, out decimal price) ? price : null
            : null;
    }

    public void RemoveAllLasExposedPrices(int consumerId)
    {
        _lastExposedPrice.TryRemove(consumerId, out _);
    }

    public void SetConsumer(int consumerId, Consumer consumer)
    {
        _consumersMemory[consumerId] = consumer;
    }

    public Consumer TryGetConsumer(int consumerId)
    {
        var result = _consumersMemory.TryGetValue(consumerId, out var cached);
        if (result)
            return cached;

        return null;
    }

    public void RemoveConsumer(int consumerId)
    {
        _consumersMemory.Remove(consumerId);
    }

    public void ResetAllCache()
    {
        _exchangeRateSnapshots = [];
        _tradingPairsSnapShot = [];
        _spreadOptionsMemory = [];
        _consumersMemory = [];
    }
}