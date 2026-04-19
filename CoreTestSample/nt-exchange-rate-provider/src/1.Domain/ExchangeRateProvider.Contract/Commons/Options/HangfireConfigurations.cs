namespace ExchangeRateProvider.Contract.Commons.Options;

public class HangfireConfigurations
{
    public JobConfigurations JobConfigurations { get; set; }
    public int RedisDatabase { get; set; }
}

public class JobConfigurations
{
}