namespace ExchangeRateProvider.Contract.Commons.Options;

public class RedisConfigurations
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public string UserName { get; set; }
}

public class RedisOption
{
    public int HangFireDatabase { get; set; } = 4;
    public int DefaultDatabase { get; set; } = 2;
}
