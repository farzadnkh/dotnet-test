using ExchangeRateProvider.Domain.Users.Entities;

namespace ExchangeRateProvider.Contract.Consumers.Dtos.Responses;

public class GetConsumerInformationModel
{
    public List<User> Users { get; set; } = new ();
}