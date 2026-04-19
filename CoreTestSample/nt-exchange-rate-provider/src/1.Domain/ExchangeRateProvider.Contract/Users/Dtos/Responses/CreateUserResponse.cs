namespace ExchangeRateProvider.Contract.Users.Dtos.Responses;

public class CreateUserResponse
{
    public int Id { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}