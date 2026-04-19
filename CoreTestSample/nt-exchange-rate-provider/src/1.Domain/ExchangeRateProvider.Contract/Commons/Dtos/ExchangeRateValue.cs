using MessagePack;

namespace ExchangeRateProvider.Contract.Commons.Dtos;

[MessagePackObject]
public class ExchangeRateValue
{
    [Key("Price")]
    public decimal Price { get; set; }

    [Key("Volume")]
    public decimal Volume { get; set; }

    [Key("Ticks")]
    public long Ticks { get; set; }
}
