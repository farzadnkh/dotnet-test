namespace ExchangeRateProvider.Contract.Users.Dtos.Requests;

public record UserPaginatedFilterRequest
{
    public string Email { get; set; }
    public string Username { get; set; }
    public bool? IsActive { get; set; }
}