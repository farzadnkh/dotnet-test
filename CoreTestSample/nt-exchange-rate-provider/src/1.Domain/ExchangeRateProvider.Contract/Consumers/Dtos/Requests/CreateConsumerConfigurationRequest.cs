using ExchangeRateProvider.Domain.Commons;

namespace ExchangeRateProvider.Contract.Consumers.Dtos.Requests;

public record CreateConsumerConfigurationRequest(
    int ConsumerId,
    bool IsActive,
    SpreadOptions SpreadOptions,
    int CreatedById,
    List<string> ExchangeRateProviderIds = null,
    List<string> MarketIds = null,
    List<string> TradingPairIds = null);
