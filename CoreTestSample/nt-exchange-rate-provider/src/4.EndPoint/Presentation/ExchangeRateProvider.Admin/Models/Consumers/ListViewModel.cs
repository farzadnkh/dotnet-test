using ExchangeRateProvider.Contract.Consumers.Dtos.Responses;

namespace ExchangeRateProvider.Admin.Models.Consumers;

public class ListViewModel
{
    public IPaginationResponse<ConsumerResponse> PaginationResponse { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public string ProjectName { get; set; }
    public string IsActive { get; set; }

    public int? Index { get; set; } = 0;
    public RequestPagingSize? Size { get; set; } = RequestPagingSize.Ten;

    public IEnumerable<SelectListItem> SizeOptions { get; set; } =
    [
         new() { Value = "5", Text = "Five" },
             new() { Value = "10", Text = "Ten"},
             new() { Value = "20", Text = "Twenty" },
             new() { Value = "50", Text = "Fifty" },
             new() { Value = "100", Text = "Hungred" }
    ];

    public IEnumerable<SelectListItem> ActiveOptions { get; set; } =
    [
         new() { Value = "", Text = "All" },
         new() { Value = "1", Text = "Yes"},
         new() { Value = "0", Text = "No" }
    ];
}
