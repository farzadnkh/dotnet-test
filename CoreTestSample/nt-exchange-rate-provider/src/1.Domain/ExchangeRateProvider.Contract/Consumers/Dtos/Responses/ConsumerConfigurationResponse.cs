using ExchangeRateProvider.Domain.Commons;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Users.Entities;

namespace ExchangeRateProvider.Contract.Consumers.Dtos.Responses;

public class ConsumerConfigurationResponse
{
    public ConsumerConfigurationResponse(
        int id,
        int consumerId,
        string consumerUsername,
        bool isActive,
        User user,
        string projectName,
        string apiKey,
        List<Provider> exchangeProviders = null,
        List<Market> markets = null,
        List<MarketTradingPair> tradingPairs = null,
        SpreadOptions spreadOptionRequest = null)
    {
        Id = id;
        TradingPairs = tradingPairs;
        IsActive = isActive;
        Markets = markets;
        ExchangeProviders = exchangeProviders;
        SpreadOptions = spreadOptionRequest;
        ConsumerId = consumerId;
        ConsumerUsername = consumerUsername;
        User = user;
        ProjectName = projectName;
        ApiKey = apiKey;
    }

    public ConsumerConfigurationResponse()
    {
    }

    public int Id { get; set; }
    public int ConsumerId { get; set; }
    public string ConsumerUsername { get; set; }
    public User User { get; set; }
    public List<Provider> ExchangeProviders { get; set; }
    public List<Market> Markets { get; set; }
    public List<MarketTradingPair> TradingPairs { get; set; }
    public bool IsActive { get; set; }
    public SpreadOptions SpreadOptions { get; set; }
    public string ProjectName { get; set; }
    public string ApiKey { get; set; }

}
