using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class JibitMatchResult : IResponseBody
{
    public required bool Matched { get; set; }
}