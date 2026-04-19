using ExchangeRateProvider.Domain.Commons.Dtos;

namespace ExchangeRateProvider.Domain.Commons.Events;

public class NewPriceChangeStreamedMessageArgs(PairExchangeRateDto dto, int consumerId, int pairId)
{
    public PairExchangeRateDto PairExchangeRate { get; set; } = dto;
    public long ConsumerId { get; set; } = consumerId;
    public long TradingPairId { get; set; } = pairId;
}