using ExchangeRateProvider.Contract.Users.Dtos.Responses;

namespace ExchangeRateProvider.Admin.Models.Users;

public class GetUserListModel : BaseListViewModel<UserResponse>
{
    public GetUserListModel(
        int id,
        string email,
        string firstName,
        string lastName,
        string userName,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        CreatedAt = createdAt;
    }

    public GetUserListModel()
    {
    }

    public int Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; }
    public string LastName { get; }
    public string UserName { get; init; }
    public DateTimeOffset CreatedAt { get; }

    public IEnumerable<SelectListItem> ActiveOptions { get; set; } =
    [
         new() { Value = "", Text = "All" },
         new() { Value = "1", Text = "Yes"},
         new() { Value = "0", Text = "No" }
    ];
}