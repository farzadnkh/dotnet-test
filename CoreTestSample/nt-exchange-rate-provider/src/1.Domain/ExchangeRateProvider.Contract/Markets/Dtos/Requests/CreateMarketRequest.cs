using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.Commons.Enums;
using ExchangeRateProvider.Domain.Markets.Enums;

namespace ExchangeRateProvider.Contract.Markets.Dtos.Requests;

public record CreateMarketRequest(
int MarketCurrencyId,
MarketCalculationTerm? MarketCalculationTerm,
bool IsDefault,
bool Published,
bool CreateAllFiats,
bool CreateAllCryptos,
List<string> ExchangeRateProviderIds,
int CreatedById,
SpreadOptions SpreadOptions = null,
RatingMethod RatingMethod = RatingMethod.Automatic);
