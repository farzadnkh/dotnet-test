using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NT.SDK.ExchangeRateProvider.Clients;
using NT.SDK.ExchangeRateProvider.Models.Enums;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Models.Responses;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Net.WebSockets;
using Websocket.Client;

namespace NT.SDK.ExchangeRateProvider.Services.StreamRateServices;

public class ExchangeRateProviderStreamRateService(
    ILogger<ExchangeRateProviderStreamRateService> logger,
    ExchangeRateProviderOptions options,
    IExchangeRateProviderApiClient client,
    IRedisDatabase redisDatabase) : IExchangeRateProviderStreamRateService
{
    private WebsocketClient _websocket;
    private static int _isRunning = 0;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly int _maxRetryCount = 5;
    private static int _retryCount = 0;

    public async Task StartStreamAsync(GetTokenRequest getTokenRequest, GetLatestPriceRequest priceRequest = null, bool renewToken = false, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
        {
            logger.LogWarning("WebSocket is already running.");
            _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Duplicate run prevention");
            return;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_websocket is null)
                _retryCount = 0;

            var token = await client.ExchangeRateProviderTokenService.GetTokenAsync(getTokenRequest, renewToken, cancellationToken);

            ArgumentException.ThrowIfNullOrWhiteSpace(token.Token);

            _websocket = CreateWebsocketClient(GenerateBaseUrl(options.BasePath, token.Token, priceRequest));

            cancellationToken.Register(() =>
            {
                logger.LogInformation("Cancellation requested. Closing WebSocket...");
                _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown requested");
                Interlocked.Exchange(ref _isRunning, 0);
            });

            SubscribeToWebSocketEvents(_websocket, getTokenRequest);

            await _websocket.Start();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting My WebSocket stream.");
            Interlocked.Exchange(ref _isRunning, 0);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SendMessageAsync(GetLatestPriceRequest priceRequest, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            Market = priceRequest.Market,
            Pairs = priceRequest.Pairs,
            ProviderTypes = priceRequest.ProviderTypes
        };

        var json = JsonConvert.SerializeObject(message);
        _websocket.Send(json);

        logger.LogInformation("Initial subscription sent: {Payload}", json);
        await Task.CompletedTask;
    }

    public async Task ReconnectAsync(GetTokenRequest getTokenRequest, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Reconnecting WebSocket...");

        _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Reconnecting");
        Interlocked.Exchange(ref _isRunning, 0);

        _retryCount++;
        await StartStreamAsync(getTokenRequest, renewToken: true, cancellationToken: cancellationToken);
    }

    #region Utilities
    private WebsocketClient CreateWebsocketClient(string url)
    {
        var uri = new Uri(url);

        if (_websocket != null)
        {
            logger.LogInformation("Cleaning up old WebSocket client.");
            _websocket.Dispose();
            _websocket = null;
        }

        return new WebsocketClient(uri)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(60),
            IsReconnectionEnabled = true
        };
    }

    private void SubscribeToWebSocketEvents(WebsocketClient ws, GetTokenRequest getTokenRequest)
    {
        ws.ReconnectionHappened.Subscribe(async info =>
        {
            if (info.Type.ToString().ToLower().Equals("lost"))
            {
                if (_maxRetryCount >= _retryCount)
                    await ReconnectAsync(getTokenRequest, default);
                else
                {
                    logger.LogInformation("Cleaning up old WebSocket client.");
                    _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Closing");
                    _websocket.Dispose();
                    _websocket = null;
                    Interlocked.Exchange(ref _isRunning, 0);
                }
            }
        });
        ws.DisconnectionHappened.Subscribe(info =>
        {
            logger.LogWarning("WebSocket disconnected: {Type} - {Reason}", info.Type, info.CloseStatusDescription ?? "Unknown");
            Interlocked.Exchange(ref _isRunning, 0);
        });

        ws.MessageReceived.Subscribe(async msg =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(msg.Text))
                    return;

                logger.LogDebug("Message received: {Message}", msg.Text);

                var message = JsonConvert.DeserializeObject<StreamPriceResponse>(msg.Text);

                switch (message.Type)
                {
                    case SocketMessageType.ConnectionOpened:
                        logger.LogInformation(message.Message);
                        break;
                    case SocketMessageType.UpdatePrice:
                        await redisDatabase.AddAsync(RedisKeys.GetStoredPriceKey(options.CachePrefix, message.Pair), GenerateResponse(message));
                        break;
                    case SocketMessageType.Hearbeat:
                        ws.Send("pong");
                        break;
                    case SocketMessageType.Exception:
                        logger.LogError("Error from server: {Message}", msg.Text);
                        break;
                    default:
                        logger.LogInformation("Unhandled message: {Message}", msg.Text);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process incoming WebSocket message.");
            }
        });
    }

    private static GetLatestPriceResponse GenerateResponse(StreamPriceResponse message)
    {
        return new GetLatestPriceResponse
                        {
                            {
                                message.Pair,
                                new()
                                {
                                    Ask = message.Ask,
                                    Bid = message.Bid,
                                    Price = message.Price,
                                    AskSpreadPercentage = message.AskSpreadPercentage,
                                    BidSpreadPercentage = message.BidSpreadPercentage,
                                    Ticks = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                                }
                            }
                        };
    }

    private static string GenerateBaseUrl(string basePath, string token, GetLatestPriceRequest priceRequest = null)
    {
        basePath = basePath.Replace("https", "wss");
        string result = $"{basePath.Trim('/')}/wss/price?access_token={token}";
        if (priceRequest is not null)
        {
            if (!string.IsNullOrEmpty(priceRequest.Market))
                result += $"&markets={priceRequest.Market}";

            if (!string.IsNullOrEmpty(priceRequest.Pairs))
                result += $"&pairs={priceRequest.Pairs}";

            if (priceRequest.ProviderTypes.HasValue && priceRequest.ProviderTypes.Value != Models.Enums.ProviderType.None)
                result += $"&provider_type={priceRequest.ProviderTypes}";
        }

        return result;
    }
    #endregion
}

