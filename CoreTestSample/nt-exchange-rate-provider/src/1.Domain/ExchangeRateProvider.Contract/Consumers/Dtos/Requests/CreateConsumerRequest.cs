namespace ExchangeRateProvider.Contract.Consumers.Dtos.Requests;

public record CreateConsumerRequest(
    string Email,
    string FirstName,
    string LastName,
    string UserName,
    string ProjectName,
    bool IsActive,
    string ApiKey,
    int CreatedById);
