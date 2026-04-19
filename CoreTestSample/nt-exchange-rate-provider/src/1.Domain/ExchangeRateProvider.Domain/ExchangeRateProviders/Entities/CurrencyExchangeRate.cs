using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using NT.DDD.Domain.Auditing.Contracts;

namespace ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

public class CurrencyExchangeRate : Entity<int>, IHasCreatedOnUtc, IHasUpdatedOnUtc
{
    public long ConsumerId { get; private set; }
    public long MarketTradingPairId { get; private set; }
    public decimal OriginalRate { get; private set; }
    public decimal Buy { get; private set; }
    public RateChangeType BuyRateChange { get; set; }
    public decimal Sell { get; private set; }
    public RateChangeType SellRateChange { get; private set; }
    public DateTimeOffset? UpdatedOnUtc { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }

    #region Constructors

    public CurrencyExchangeRate(
        long consumerId,
        long marketTradingPairId,
        decimal originalRate,
        decimal buy,
        RateChangeType buyRateChange,
        decimal sell,
        RateChangeType sellRateChange,
        DateTimeOffset createdOnUtc)
    {
        ConsumerId = consumerId;
        MarketTradingPairId = marketTradingPairId;
        OriginalRate = originalRate;
        Buy = buy;
        BuyRateChange = buyRateChange;
        Sell = sell;
        SellRateChange = sellRateChange;
        CreatedOnUtc = createdOnUtc;
    }

    public CurrencyExchangeRate()
    {
    }

    #endregion

    #region Behaviors
    public void UpdateBuyRate(decimal newBuyRate, RateChangeType changeType)
    {
        Buy = newBuyRate;
        BuyRateChange = changeType;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateSellRate(decimal newSellRate, RateChangeType changeType)
    {
        Sell = newSellRate;
        SellRateChange = changeType;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void SetOriginalRate(decimal rate)
    {
        OriginalRate = rate;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }
    #endregion
}

