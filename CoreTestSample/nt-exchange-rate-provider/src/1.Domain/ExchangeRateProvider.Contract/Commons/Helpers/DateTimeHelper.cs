namespace ExchangeRateProvider.Contract.Commons.Helpers
{
    public static class DateTimeHelper
    {
        public static string ToFormatedDateTime(this DateTimeOffset createdOnUtc, string format = "MM/dd/yyyy")
            => createdOnUtc.ToString(format);
    }
}
