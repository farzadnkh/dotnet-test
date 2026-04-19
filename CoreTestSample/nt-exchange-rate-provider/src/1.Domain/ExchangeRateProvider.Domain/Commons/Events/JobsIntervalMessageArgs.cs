namespace ExchangeRateProvider.Domain.Commons.Events;

public class JobsIntervalMessageArgs
{
    public IntervalTypes IntervalTypes { get; set; }

    public int CryptoJobInterval { get; set; } = 1;
    public int FiatsJobInterval { get; set; } = 5;
}

public enum IntervalTypes
{
    All = 1,
    Crypto = 2,
    Fiats = 3
}
