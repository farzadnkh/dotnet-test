using ExchangeRateProvider.Domain.Currencies.Enums;

namespace ExchangeRateProvider.Contract.Currencies.Dtos.Requests
{
    public record CurrencyPaginatedFilterRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public CurrencyType? Type { get; set; }
        public bool? Published { get; set; }
    }
}
