using Newtonsoft.Json;

namespace NT.KYC.Jibit.Models.Responses.ResponseModels;

public class IbanInfo
{
    /// <summary>
    /// The name of the bank.
    /// </summary>
    [JsonProperty(PropertyName = "bank")]
    public string? Bank { get; set; }

    /// <summary>
    /// The deposit number associated with the IBAN.
    /// </summary>
    [JsonProperty(PropertyName = "depositNumber")]
    public string? DepositNumber { get; set; }

    /// <summary>
    /// The International Bank Account Number (IBAN).
    /// </summary>
    [JsonProperty(PropertyName = "iban")]
    public string? Iban { get; set; }

    /// <summary>
    /// The status of the IBAN (e.g., "ACTIVE").
    /// </summary>
    [JsonProperty(PropertyName = "status")]
    public string? Status { get; set; }

    /// <summary>
    /// A list of owners associated with the IBAN.
    /// </summary>
    [JsonProperty(PropertyName = "owners")]
    public List<Owner>? Owners { get; set; }
}