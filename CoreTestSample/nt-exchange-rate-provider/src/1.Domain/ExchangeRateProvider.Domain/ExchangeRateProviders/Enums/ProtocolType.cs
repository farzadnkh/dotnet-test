using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;

public enum ProtocolType
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Api call")]
    ApiCall = 2,

    [Display(Name = "Web Socket")]
    WebSocket = 3
}
