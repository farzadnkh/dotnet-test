namespace NT.KYC.Jibit.Utils;

public static class JibitEndPoints
{
    public static readonly string BaseUrl = "https://napi.jibit.ir/ide";

    public const string GetAccessToken = "/v1/tokens/generate";

    public const string RefreshAccessToken = "/v1/tokens/refresh";

    public const string GetIdentityDetails = "/v1/services/identity";

    public const string GetMatchingResult = "v1/services/matching";
    
    public const string CardsService = "v1/cards";
}