using ExchangeRateProvider.Domain.Currencies.Entities;

namespace ExchangeRateProvider.Contract.Commons.Helpers
{
    public static class CurrencyHelper
    {
        public static string GetCurrencyName(this Currency currency)
        {
            if (currency is null)
                return "";

            return $"{currency.Name} ({currency.Code})";
        }
    }
}
