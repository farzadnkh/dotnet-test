using ExchangeRateProvider.Contract.Markets.Dtos.Responses;

namespace ExchangeRateProvider.Admin.Models.ManualRatings;

public class ManualRatingListModel
{
    public List<ManualRatingResponse> ManualRatingResponses { get; set; }
}
