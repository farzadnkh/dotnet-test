namespace ExchangeRateProvider.Contract.Users.Dtos.Responses;

public class UserResponse
{
    public UserResponse(
        int id,
        string email,
        string firstName,
        string lastName,
        string userName,
        string createdAt)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        CreatedAt = createdAt;
    }

    public UserResponse()
    {
    }

    public int Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init;}
    public string UserName { get; init; }
    public bool IsActive { get; init; }
    public string CreatedAt { get; init;}
}