using Newtonsoft.Json;
using NT.KYC.Jibit.Models.Responses.ResponseModels;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Models.Responses;

public class CardNumberToIbanResponse : IResponseBody
{
    /// <summary>
    /// A unique identifier for the response.
    /// </summary>
    [JsonProperty(PropertyName = "number")]
    public string? Number { get; set; }

    /// <summary>
    /// The type of transaction (e.g., "DEBIT").
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public string? Type { get; set; }

    /// <summary>
    /// Information about the IBAN associated with the transaction.
    /// </summary>
    [JsonProperty(PropertyName = "ibanInfo")]
    public IbanInfo? IbanInfo { get; set; }
    
    /// <summary>
    /// Information about the card associated with the transaction.
    /// </summary>
    [JsonProperty(PropertyName = "cardInfo")]
    public CardInfo? CardInfo { get; set; }
}