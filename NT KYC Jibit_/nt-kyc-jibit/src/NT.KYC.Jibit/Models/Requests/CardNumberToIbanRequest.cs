using Newtonsoft.Json;
using NT.SDK.RestClient.Helpers;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Requests;

public class CardNumberToIbanRequest : IRequestBody
{
    [QueryParameter]
    [JsonProperty(PropertyName = "cardNumber")]
    public required string CardNumber { get; set; }

    /// <summary>
    /// if true the iban info will come else the card info will come.
    /// </summary>
    [QueryParameter]
    [JsonProperty(PropertyName = "iban")]
    public bool Iban { get; set; } = true;
}