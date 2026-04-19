using NT.SDK.RestClient.Models;

namespace NT.SDK.ExchangeRateProvider.Models.Responses;

public class GetLatestPriceResponse : Dictionary<string, PriceDetails>, IResponseBody
{
}

public class PriceDetails
{
    public decimal Price { get; set; }

    /// <summary>
    /// Ask = upper price
    /// </summary>
    public decimal? Ask { get; set; }
    public decimal? AskSpreadPercentage { get; set; }

    /// <summary>
    /// Bid = lower price
    /// </summary>
    public decimal? Bid { get; set; }
    public decimal? BidSpreadPercentage { get; set; }

    public long Ticks { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

