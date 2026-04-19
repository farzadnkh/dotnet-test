using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;

namespace ExchangeRateProvider.Contract.Markets.Dtos.Requests;

public record CreateMarketTradingPairRequest(
int MarketId,
int CurrencyId,
bool Published,
string Description,
List<string> ExchangeRateProviderIds,
int CreatedById,
SpreadOptions SpreadOptions = null,
RatingMethod RatingMethod = RatingMethod.None);
