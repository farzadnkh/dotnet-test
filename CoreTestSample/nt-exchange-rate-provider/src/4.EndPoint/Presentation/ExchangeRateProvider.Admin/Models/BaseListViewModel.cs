namespace ExchangeRateProvider.Admin.Models
{
    public abstract class BaseListViewModel<TResponseDto>
        where TResponseDto : class, new()
    {
        public IPaginationResponse<TResponseDto> PaginationResponse { get; set; }

        public string Published { get; set; }
        public int? Index { get; set; } = 0;
        public RequestPagingSize? Size { get; set; } = RequestPagingSize.Ten;

        public IEnumerable<SelectListItem> PublishOptions { get; set; } = [];

        public IEnumerable<SelectListItem> SizeOptions { get; set; } =
        [
             new() { Value = "5", Text = "Five" },
             new() { Value = "10", Text = "Ten"},
             new() { Value = "20", Text = "Twenty" },
             new() { Value = "50", Text = "Fifty" },
             new() { Value = "100", Text = "Hungred" }
        ];
    }
}
