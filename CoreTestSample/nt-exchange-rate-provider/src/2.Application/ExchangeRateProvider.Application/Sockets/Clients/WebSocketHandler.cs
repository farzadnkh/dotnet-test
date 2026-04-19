using Duende.IdentityServer.Validation;
using ExchangeRateProvider.Application.Commons.Helpers;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Contract.Settings.Services;
using ExchangeRateProvider.Domain.Commons.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Net.WebSockets;
using System.Text;

namespace ExchangeRateProvider.Application.Sockets.Clients;

public class WebSocketHandler(
    ILogger<WebSocketHandler> logger,
    IAggregatorServiceFactory aggregatorServiceFactory,
    EncryptionConfiguration encryptionConfiguration,
    IRedisDatabase redisDatabase,
    IExchangeRateSnapshotMemory exchangeRateSnapshotMemory,
    ISettingService settingService,
    ISettingQueryRepository settingQueryRepository,
    ITokenValidator tokenValidator)
{
    private long _lastActivityUnixMs;
    private bool _shouldApplyNewFilter = false;
    private readonly Lock _lock = new();

    private string _consumerClientId = string.Empty;


    #region Handle
    public async Task HandleAsync(HttpContext context, WebSocket socket)
    {
        string token = context.Request.Query["access_token"].ToString();

        if (!await ValidatTokenAsync(token))
        {
            await socket.CloseOutputAsync(WebSocketCloseStatus.PolicyViolation, "Invalid Token", CancellationToken.None);
            return;
        }

        var isBlocked = await redisDatabase.SetContainsAsync(RedisKeys.DeActivatedConsumers(), ExtractConsumerId());
        if (isBlocked)
        {
            await socket.CloseOutputAsync(
                WebSocketCloseStatus.PolicyViolation,
                "Consumer Is Blocked, Please contact your administrator.",
                CancellationToken.None);
            return;
        }

        var clientConnection = CreateClientConnection(context, socket, token);
        logger.LogInformation("Client connected: {ConnectionId}", clientConnection.ConnectionId);

        if (socket.State == WebSocketState.Open)
        {
            await SendMessageAsync(socket,
                    GetSystemMessage(SocketMessageType.ConnectionOpened, "Connection Opened, now Your are going to Receive Messages."),
                    WebSocketMessageType.Text,
                    true,
                    default);
            _lastActivityUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        try
        {
            Task clientReciveAsync = ReceiveMessagesAsync(clientConnection.Client, clientConnection.ConnectionId);
            Task sreamPrices = StreamPricesToClientAsync(clientConnection.Client, context);
            Task heartbeatMonitor = SendHeartbeatAsync(clientConnection.Client);

            await Task.WhenAll(clientReciveAsync, sreamPrices, heartbeatMonitor);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "WebSocket client error");
        }
    }
    #endregion

    #region HearBeat
    private async Task SendHeartbeatAsync(ClientConnection client)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(client.TokenSource.Token))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timeoutThreshold = 90_000;
            var inactiveTime = now - timeoutThreshold;

            if (_lastActivityUnixMs < inactiveTime)
            {
                await SendMessageAsync(
                        client.Socket,
                        GetSystemMessage(SocketMessageType.ConnectionClosed, "No activity detected. Closing connection."),
                        WebSocketMessageType.Text,
                        true,
                        client.TokenSource.Token);

                logger.LogWarning("Client timed out after {Seconds}s inactivity", inactiveTime / 1000);
                client.TokenSource.Cancel();
                timer.Dispose();
                await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Inactivity", CancellationToken.None);
                break;
            }

            if (client.Socket.State != WebSocketState.Open)
            {
                logger.LogInformation("Socket for client {ConnectionId} is closed. Stopping heartbeat.", client.AccessToken);
                timer.Dispose();
                break;
            }

            try
            {
                var message = new
                {
                    Type = SocketMessageType.Hearbeat,
                    Message = "ping",
                    TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await SendMessageAsync(
                    client.Socket,
                    GetSystemMessage(message),
                    WebSocketMessageType.Text,
                    true,
                    client.TokenSource.Token);
                logger.LogDebug("Sent heartbeat to client {ConnectionId}", client.AccessToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Heartbeat sending canceled for {ConnectionId}", client.AccessToken);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending heartbeat to client {ConnectionId}", client.AccessToken);
                break;
            }
        }
    }
    #endregion

    #region ReceiveMessages
    private async Task ReceiveMessagesAsync(ClientConnection client, string connectionId)
    {
        var buffer = new byte[4096];
        while (client.Socket.State == WebSocketState.Open && !client.TokenSource.Token.IsCancellationRequested)
        {
            try
            {
                WebSocketReceiveResult result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), client.TokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.LogInformation("Client {ConnectionId} initiated close (Code: {CloseStatus}, Reason: {CloseReason}).",
                        connectionId, result.CloseStatus, result.CloseStatusDescription);

                    if (client.Socket.State == WebSocketState.CloseReceived)
                    {
                        await ColoseConnectionAsync(
                             client,
                             SocketMessageType.ConnectionClosed,
                             WebSocketCloseStatus.NormalClosure,
                             "Client initiated close",
                             "Client initiated close");
                        break;
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (message.Contains("pong", StringComparison.OrdinalIgnoreCase))
                    {
                        var isBlocked = await redisDatabase.SetContainsAsync(RedisKeys.DeActivatedConsumers(), ExtractConsumerId());
                        if (isBlocked)
                        {
                            await ColoseConnectionAsync(
                               client,
                               SocketMessageType.Error,
                               WebSocketCloseStatus.PolicyViolation,
                               "Blocked",
                               "Consumer Is Blocked");
                            return;
                        }
                        if (!await ValidatTokenAsync(client.AccessToken))
                        {
                            await ColoseConnectionAsync(
                                client,
                                SocketMessageType.Error,
                                WebSocketCloseStatus.PolicyViolation,
                                "Unauthorized",
                                "Invalid Token");
                            return;
                        }

                        _lastActivityUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        continue;
                    }

                    var filters = JsonConvert.DeserializeObject<GetLatestTickRequest>(message);
                    logger.LogDebug("Text message received from client {ConnectionId}: {Message}", connectionId, message);

                    client.MessageQueue = filters;

                    lock (_lock)
                        _shouldApplyNewFilter = true;
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    logger.LogDebug("Binary message received from client {ConnectionId}.", connectionId);
                }
            }
            catch (JsonException jEx)
            {
                logger.LogCritical(jEx, "Wrong Message Format Detected. Valid Message Should be Json Format");
                await SendMessageAsync(client.Socket, "Wrong Message Format Detected, Please Send Valid Message.", WebSocketMessageType.Text, true, client.TokenSource.Token);
                continue;
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("ReceiveMessagesAsync for {ConnectionId} canceled.", connectionId);
                break;
            }
            catch (WebSocketException ex)
            {
                logger.LogWarning(ex, "WebSocket error during receive for client {ConnectionId}: {ErrorCode}", connectionId, ex.WebSocketErrorCode);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error in ReceiveMessagesAsync for client {ConnectionId}", connectionId);
                break;
            }
        }
        logger.LogInformation("ReceiveMessagesAsync for {ConnectionId} finished.", connectionId);
    }
    #endregion

    #region StreamPrices
    private async Task StreamPricesToClientAsync(ClientConnection client, HttpContext context)
    {
        SettingModel currentSettingsInUse = await settingService.LoadSettingsAsync(settingQueryRepository);
        TimeSpan currentInterval = TimeSpan.FromMilliseconds(currentSettingsInUse.SnapshotClockTime == 0 ? 10000 : currentSettingsInUse.SnapshotClockTime);
        PeriodicTimer pc = new(currentInterval);

        var consumerId = ExtractConsumerId();

        while (await pc.WaitForNextTickAsync(client.TokenSource.Token))
        {
            TimeSpan latestInterval = TimeSpan.FromMilliseconds(settingService.SnapshotClockTime == 0 ? 10000 : settingService.SnapshotClockTime);
            if (currentInterval != latestInterval)
            {
                logger.LogInformation("Detected interval change from {OldInterval} to {NewInterval}. Recreating timer for client {ConnectionId}.", currentInterval, latestInterval, client.AccessToken);

                pc.Dispose();

                currentInterval = latestInterval;
                pc = new PeriodicTimer(currentInterval);
                continue;
            }

            if (client.Socket.State == WebSocketState.Closed)
            {
                logger.LogInformation("Socket Is closed. Timer will stop and wont send  Any data to client Any more.");
                pc.Dispose();
                return;
            }

            var filter = CreateFilterDto(client.MessageQueue);
            if (filter is null) continue;

            IAsyncEnumerable<PairExchangeRateDto> pricesEnumerable;
            await client.QueueLock.WaitAsync(client.TokenSource.Token);
            try
            {
                logger.LogInformation("Processing client message: {Message}", client.MessageQueue);

                
                pricesEnumerable = aggregatorServiceFactory.GetPairExchangeRatesForConsumerAllPairsAsync(consumerId, filter, _shouldApplyNewFilter, client.TokenSource.Token);

                if (_shouldApplyNewFilter)
                    lock (_lock)
                        _shouldApplyNewFilter = false;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize price stream for client {ConnectionId}. Disconnecting.", client.AccessToken);
                client.TokenSource.Cancel();
                return;
            }
            finally
            {
                client.QueueLock.Release();
            }

            try
            {
                await foreach (var price in pricesEnumerable.WithCancellation(client.TokenSource.Token))
                {
                    if (price is null) 
                        continue;

                    if (client.Socket.State != WebSocketState.Open)
                    {
                        logger.LogInformation("Socket for client {ConnectionId} is closed. Ending stream.", client.AccessToken);
                        break;
                    }

                    var dto = new SocketMessage
                    {
                        Type = 20,
                        Pair = price.CurrencyPair,
                        Price = price.Price,
                        Ask = price.UpperLimit,
                        AskSpreadPercentage = price.UpperLimitPercentage,
                        Bid = price.LowerLimit,
                        BidSpreadPercentage = price.LowerLimitPercentage
                    };

                    var message = JsonConvert.SerializeObject(dto);

                    try
                    {
                        await SendMessageAsync(client.Socket, message, WebSocketMessageType.Text, true, client.TokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogDebug("Send canceled for {ConnectionId}.", client.AccessToken);
                        break;
                    }
                    catch (WebSocketException ex) when (
                        ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                        ex.WebSocketErrorCode == WebSocketError.InvalidState)
                    {
                        logger.LogInformation("WebSocket exception for client {ConnectionId}: {Code}. Ending stream.", client.AccessToken, ex.WebSocketErrorCode);
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unexpected error sending message to {ConnectionId}.", client.AccessToken);
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Streaming canceled for {ConnectionId}.", client.AccessToken);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during streaming for {ConnectionId}.", client.AccessToken);
                break;
            }

            logger.LogInformation("StreamPricesToClientAsync iteration for {ConnectionId} completed.", client.AccessToken);
        }

        logger.LogInformation("StreamPricesToClientAsync loop for {ConnectionId} exited.", client.AccessToken);
    }

    #endregion

    #region Utilities
    private (string ConnectionId, ClientConnection Client) CreateClientConnection(HttpContext context, WebSocket socket, string token)
    {
        var connectionId = Guid.NewGuid().ToString();
        var client = new ClientConnection
        {
            Socket = socket,
            AccessToken = token,
        };
        client.MessageQueue = ReadFilterRequestFromQueryString(context.Request.Query);

        return (connectionId, client);
    }

    private int ExtractConsumerId() => TokenHelper.GetConsumerId(_consumerClientId, encryptionConfiguration.EncryptionKey);

    private static GetExchangeRateForAllPairsWithFilter CreateFilterDto(GetLatestTickRequest request)
    {
        if (request == null) return null;

        var result = new GetExchangeRateForAllPairsWithFilter();

        if (!string.IsNullOrWhiteSpace(request.Market))
            result.Markets = [.. request.Market.Trim().Split(',')];

        if (!string.IsNullOrWhiteSpace(request.Pairs))
            result.Pairs = [.. request.Pairs.Trim().Split(',')];

        result.ProviderTypes = request.ProviderTypes ?? ProviderType.None;

        if (result.Markets.Count == 0 &&
            result.Pairs.Count == 0 &&
            result.ProviderTypes == ProviderType.None)
            return null;

        return result;
    }

    private async Task<bool> ValidatTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        TokenValidationResult validation = await tokenValidator.ValidateAccessTokenAsync(token);
        
        if (!validation.IsError)
            _consumerClientId = validation?.Client?.ClientId;

        return validation.Claims is not null;
    }

    private GetLatestTickRequest ReadFilterRequestFromQueryString(IQueryCollection query)
    {
        var providerType = Enum.TryParse<ProviderType>(query["provder_type"].ToString(), true, out var type);
        if (!providerType) type = ProviderType.None;

        GetLatestTickRequest result = new()
        {
            Market = query["market"].ToString(),
            Pairs = query["pairs"].ToString(),
            ProviderTypes = type,
        };

        if (result.Market is not null ||
            result.Market is not null ||
            result.Market is not null)
        {
            lock (_lock)
                _shouldApplyNewFilter = true;
        }

        return result;

    }

    private static async Task SendMessageAsync(
        WebSocket sokcet,
        string message,
        WebSocketMessageType messageType,
        bool endOfMessage,
        CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await sokcet.SendAsync(new ArraySegment<byte>(buffer), messageType, endOfMessage, cancellationToken);
    }

    private async Task ColoseConnectionAsync(
    ClientConnection client,
    SocketMessageType socketMessageType,
    WebSocketCloseStatus webSocketCloseStatus,
    string message,
    string statusDescription)
    {
        var consumerId = ExtractConsumerId();
        exchangeRateSnapshotMemory.RemoveAllMemoryForConsumer(consumerId);
        exchangeRateSnapshotMemory.RemoveAllLasExposedPrices(consumerId);

        await SendMessageAsync(client.Socket, GetSystemMessage(socketMessageType, message), WebSocketMessageType.Text, true, default);
        client.TokenSource.Cancel();
        await client.Socket.CloseOutputAsync(webSocketCloseStatus, statusDescription, CancellationToken.None);
    }

    private static string GetSystemMessage(SocketMessageType messageType, string messageText)
    {
        var message = new
        {
            Type = messageType,
            Message = messageText
        };

        return JsonConvert.SerializeObject(message);
    }

    private static string GetSystemMessage(object messageObject) => JsonConvert.SerializeObject(messageObject);
    #endregion
}

public class SocketMessage
{
    public int Type { get; set; }
    public string Pair { get; set; }
    public decimal Price { get; set; }
    public decimal? Ask { get; set; }
    public decimal? AskSpreadPercentage { get; set; }
    public decimal? Bid { get; set; }
    public decimal? BidSpreadPercentage { get; set; }
    public string Message { get; set; }
}

public class ClientConnection
{
    public required WebSocket Socket { get; init; }
    public required string AccessToken { get; init; }
    public CancellationTokenSource TokenSource { get; } = new();
    public DateTime ConnectedAt { get; } = DateTime.UtcNow;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public GetLatestTickRequest MessageQueue { get; set; } = null;
    public SemaphoreSlim QueueLock { get; } = new(1, 1);
}

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