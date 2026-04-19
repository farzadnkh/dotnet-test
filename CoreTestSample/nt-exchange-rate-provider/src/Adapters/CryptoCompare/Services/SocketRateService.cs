using ExchangeRateProvider.Adapter.Base.Services;
using ExchangeRateProvider.Adapter.CryptoCompare.Models.Requests.Sockets;
using ExchangeRateProvider.Adapter.CryptoCompare.Models.Responses.Sockets;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Commons.Services;
using ExchangeRateProvider.Contract.ExchangeRateProviders;
using ExchangeRateProvider.Contract.ExchangeRateProviders.Dtos.Responses;
using ExchangeRateProvider.Contract.Markets;
using ExchangeRateProvider.Domain.Currencies.Enums;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using ExchangeRateProvider.Domain.Markets.Entities;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NT.DDD.Application.Exceptions;
using System.Net.WebSockets;
using Websocket.Client;

namespace ExchangeRateProvider.Adapter.CryptoCompare.Services;

public class SocketRateService(
    IProviderBusinessLogicQueryRepository providerBusinessLogicQueryRepository,
    BaseUri baseUri,
    EncryptionConfiguration encryptionConfiguration,
    IMarketTradingPairQueryRepository tradingPairQueryRepository,
    IProviderApiAccountQueryRepository providerApiAccountQueryRepository,
    IRedisService redisService,
    ILogger<SocketRateService> logger) : AdapterBaseService(providerBusinessLogicQueryRepository,
                                                              providerApiAccountQueryRepository,
                                                              encryptionConfiguration,
                                                              tradingPairQueryRepository,
                                                              logger)
{
    private const string SubscriptionAction = "SUBSCRIBE";
    private const string SubscriptionType = "spot_v1_latest_tick";
    private List<string> _cachedFormattedPairs = [];
    private IEnumerable<ProviderBusinessLogic> _cachedProviderLogics = [];
    private static int _isRunning = 0;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private WebsocketClient _websocket;

    [Queue("socket_call")]
    [DisableConcurrentExecution(timeoutInSeconds: 10)]
    public async Task StreamRates(int providerApiAccountId, CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
        {
            logger.LogWarning("CryptoCompare WebSocket is already running.");
            _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown requested");
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            (int providerId, ProviderApiAccountCredentials credentials) = await GetAccountApiKey(ProviderType.CryptoCompare, providerApiAccountId, cancellationToken);

            if (credentials?.ApiKey is null)
            {
                logger.LogCritical("There is no Api Key Sets for The Provider with Type: {Type}", ProviderType.CryptoCompare);
                return;
            }

            IEnumerable<MarketTradingPair> pairs = await GetPairs(CurrencyType.Crypto, providerId, cancellationToken);
            var formattedPairs = FormatTradingPairs(pairs);
            _cachedFormattedPairs.AddRange(formattedPairs);

            _cachedProviderLogics = await GetAllBusinessLogicsAsync(providerId);

            _websocket = CreateWebsocketClient(credentials.ApiKey);

            cancellationToken.Register(() =>
            {
                logger.LogInformation("Cancellation requested. Closing WebSocket...");
                _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown requested");
                Interlocked.Exchange(ref _isRunning, 0);
            });

            await _websocket.Start();
            foreach (var logic in _cachedProviderLogics)
                await SendInitialSubscription(_websocket, formattedPairs, logic.Name.ToLower());

            SubscribeToWebSocketEvents(_websocket);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while starting WebSocket stream.");
            Interlocked.Exchange(ref _isRunning, 0);
        }
        finally
        {
            _lock.Release();
        }
    }

    private WebsocketClient CreateWebsocketClient(string apiKey)
    {
        if (_websocket != null && _websocket.IsRunning)
        {
            logger.LogInformation("WebsocketClient already exists and is running/connected. Not creating a new one or connecting again.");
            return _websocket;
        }

        if (_websocket != null)
        {
            logger.LogInformation("Existing WebsocketClient is not running. Disposing and creating a new one.");
            _websocket.Dispose();
            _websocket = null;
        }


        var uri = new Uri(UrlGenerator.GenerateCryptoCompareSocketUrl(baseUri.CryptoCompareProviderBaseUri, apiKey));
        return new WebsocketClient(uri)
        {
            ReconnectTimeout = TimeSpan.FromSeconds(30),
            IsReconnectionEnabled = true
        };
    }

    private async Task SendInitialSubscription(WebsocketClient ws, List<string> pairs, string market)
    {
        var subscription = new SendSubscriptionRequest
        {
            Action = SubscriptionAction,
            Groups = ["VALUE", "CURRENT_HOUR", "CURRENT_DAY"],
            Type = SubscriptionType,
            Market = market,
            Instruments = pairs,
            TickInterval = 1
        };

        ws.Send(JsonConvert.SerializeObject(subscription));
        logger.LogInformation("Sent initial subscription for {Count} pairs.", pairs.Count);
        await Task.CompletedTask;
    }

    private void SubscribeToWebSocketEvents(WebsocketClient ws)
    {
        ws.ReconnectionHappened.Subscribe(async info =>
        {
            logger.LogInformation("WebSocket reconnected: {Type}", info.Type);

            if (_cachedFormattedPairs != null && _cachedFormattedPairs.Any())
            {
                try
                {
                    foreach (var logic in _cachedProviderLogics)
                        await SendInitialSubscription(_websocket, _cachedFormattedPairs, logic.Name);

                    logger.LogInformation("Re-subscription after reconnect succeeded.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to re-subscribe after WebSocket reconnection.");
                }
            }
        });

        ws.DisconnectionHappened.Subscribe(info =>
        {
            logger.LogWarning("WebSocket disconnected: {Type} - {Reason}", info.Type, info.CloseStatusDescription ?? "Disconnected");
            Interlocked.Exchange(ref _isRunning, 0);
        });

        ws.MessageReceived.Subscribe(async msg =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(msg.Text)) return;

                var baseMessage = JsonConvert.DeserializeObject<InboundMessageBase>(msg.Text);
                switch (baseMessage?.Type)
                {
                    case "952":
                        var trade = JsonConvert.DeserializeObject<TradeMessageResponse>(msg.Text);
                        await redisService.SavePairsPriceDataToRedisAsync(ProviderType.CryptoCompare, trade.Instrument, trade.Price, 0, trade.Market);
                        break;
                    case "959":
                    case "1102":
                        var tick = JsonConvert.DeserializeObject<AggregateTickMessageResponse>(msg.Text);
                        await redisService.SavePairsPriceDataToRedisAsync(ProviderType.CryptoCompare, tick.Instrument, tick.Price, tick.CurrentDayVolume, tick.Market);
                        break;
                    case "4013":
                        break;
                    case "4000":
                        logger.LogInformation("WelcomMessage: {message}", msg.Text);
                        break;
                    case "4004":
                        logger.LogError("PairErrorMessage: {message}", msg.Text);
                        break;
                    case "4006":
                        logger.LogError("SubscriptionRejected: {message}", msg.Text);
                        break;
                    case "4002":
                        logger.LogCritical("{message}", msg.Text);
                        _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown requested");
                        await _websocket.Reconnect();
                        break;
                    case "4012":
                        logger.LogCritical("Something is wrong: {message}", msg.Text);
                        _websocket?.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown requested");
                        throw ApplicationBadRequestException.Create("Crypto Compare Job failed. Check Log For more Information.");
                    default:
                        logger.LogInformation("Unhandled WebSocket message: {Message}", msg.Text);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing WebSocket message.");
            }
        });
    }
}