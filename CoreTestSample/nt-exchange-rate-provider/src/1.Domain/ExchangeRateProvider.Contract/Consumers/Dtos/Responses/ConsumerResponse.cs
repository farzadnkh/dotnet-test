namespace ExchangeRateProvider.Contract.Consumers.Dtos.Responses;

public class ConsumerResponse
{
    public ConsumerResponse(
        int id,
        string email,
        string firstName,
        string lastName,
        string userName,
        string projectName,
        bool isActive,
        string createdAt, 
        bool isSuccess = true,
        string error = "")
    {
        Id = id;
        Email = email;
        UserName = userName;
        ProjectName = projectName;
        CreatedAt = createdAt;
        IsActive = isActive;
        FirstName = firstName;
        LastName = lastName;
        IsSuccess = isSuccess;
        ErrorMessage = error;
    }

    public ConsumerResponse()
    {
    }

    public int Id { get; init; }
    public string Email { get; init; }
    public string UserName { get; init; }
    public string ProjectName { get; init; }
    public string CreatedAt { get; init; }
    public bool IsActive { get; init; }
    public  string FirstName { get; init; }
    public string LastName { get; init; }
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; }
}
