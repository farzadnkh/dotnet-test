namespace ExchangeRateProvider.Contract.Commons.Helpers
{
    public static class MathHelper
    {
        public static decimal Ratio(decimal x, decimal y)
        {
            if(x == 0 || y == 0)
                return 0;

            return x / y;
        }

        public static decimal TruncateDecimal(this decimal number, int? digits)
        {
            if(digits is null)
                return number;

            decimal factor = (decimal)Math.Pow(10, digits.Value);
            return Math.Truncate(number * factor) / factor;
        }
    }
}
