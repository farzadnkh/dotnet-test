using Newtonsoft.Json;
using NT.SDK.RestClient.Models;

namespace NT.SDK.ExchangeRateProvider.Models.Responses
{
    public class GetTokenResponse : IResponseBody
    {
        [JsonProperty("accessToken")]
        public string Token { get; set; }

        [JsonProperty("expireInSec")]
        public int ExpireInSec { get; set; }
    }
}
