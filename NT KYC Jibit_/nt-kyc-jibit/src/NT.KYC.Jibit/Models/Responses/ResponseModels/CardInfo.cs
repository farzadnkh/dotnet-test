using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class CardInfo
{
    /// <summary>
    /// The name of the bank.
    /// </summary>
    [JsonProperty(PropertyName = "bank")]
    public string? Bank { get; set; }

    /// <summary>
    /// The type of card (e.g., "DEBIT").
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public string? Type { get; set; }

    /// <summary>
    /// The name of the card owner.
    /// </summary>
    [JsonProperty(PropertyName = "ownerName")]
    public string? OwnerName { get; set; }

    /// <summary>
    /// The deposit number associated with the card.
    /// </summary>
    [JsonProperty(PropertyName = "depositNumber")]
    public string? DepositNumber { get; set; }
}