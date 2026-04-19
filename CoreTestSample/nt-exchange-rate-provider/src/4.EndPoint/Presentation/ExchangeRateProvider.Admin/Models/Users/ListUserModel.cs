namespace ExchangeRateProvider.Admin.Models.Users;

public class ListUserModel : BaseListViewModel<GetUserListModel>
{
    public int Id { get; set; }
    public int Email { get; set; }
    public int Username { get; set; }
}