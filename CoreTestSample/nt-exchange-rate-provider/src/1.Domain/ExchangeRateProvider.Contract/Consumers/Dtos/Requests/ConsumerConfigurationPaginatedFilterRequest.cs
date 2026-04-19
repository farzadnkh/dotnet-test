namespace ExchangeRateProvider.Contract.Consumers.Dtos.Requests;

public record ConsumerConfigurationPaginatedFilterRequest
{
    public int ConsumerId {  get; set; }
    public int ProviderId { get; set; }
    public int MarketId { get; set; }
    public int PairId { get; set; }
    public bool? IsActive { get; set; }
}
