namespace ExchangeRateProvider.Contract.Consumers.Dtos.Requests;

public record ConsumerPaginatedFilterRequest
{
    public string ProjectName { get; set; }
    public string UserName { get; set; }
    public bool? IsActive { get; set; }
}
