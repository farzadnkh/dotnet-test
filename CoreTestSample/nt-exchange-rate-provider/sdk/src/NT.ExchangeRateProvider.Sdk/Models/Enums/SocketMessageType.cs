namespace NT.SDK.ExchangeRateProvider.Models.Enums;

public enum SocketMessageType
{
    None = 0,
    OpeningConnection = 1,
    Hearbeat = 2,
    ClosingConnection = 3,

    ConnectionOpened = 10,
    ConnectionClosed = 13,

    UpdatePrice = 20,
    Error = 21,

    Exception = 50
}
