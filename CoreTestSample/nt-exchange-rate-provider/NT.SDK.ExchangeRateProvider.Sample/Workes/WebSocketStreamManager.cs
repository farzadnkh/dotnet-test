using Hangfire;
using NT.SDK.ExchangeRateProvider.Models.Options;
using NT.SDK.ExchangeRateProvider.Models.Requests;
using NT.SDK.ExchangeRateProvider.Services.StreamRateServices;
using System.Collections.Concurrent;

namespace NT.SDK.ExchangeRateProvider.Sample.Workes;

public class WebSocketStreamManager(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager,
    ExchangeRateProviderOptions  options,
    ILogger<WebSocketStreamManager> logger) : IHostedLifecycleService
{
    private readonly ConcurrentDictionary<string, string> _jobIds = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("WebSocketStreamManager StartAsync completed.");
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        RegisterStartSocketJob(cancellationToken);
        logger.LogInformation("Reconnect job registered in StartingAsync.");
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        RegisterReconnectJob(cancellationToken);
        logger.LogInformation("WebSocketStreamManager StartedAsync completed.");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("WebSocketStreamManager is stopping. Removing jobs.");
        RemoveReconnectJob();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("WebSocketStreamManager has completely stopped.");
        return Task.CompletedTask;
    }

    private void RegisterStartSocketJob(CancellationToken cancellation)
    {
        var tokenRequest = new GetTokenRequest()
        {
            ClientId = "ZzhFkoKfr4o_GbiKTqMduob1xhOQeSnXfhrf2bq1lJBMmocAiOgWAC3B8M6hN305",
            ClientSecret = "xV7akVEkpG8n0NO4prjK8qBtPFfIlBGLktHmUi19g57eGVl470RY1cDFu1AmRjGbjcMR31uq/TGXuW4PNgy4nQ==",
            Scopes = "realtime-api"
        };
        var id = backgroundJobClient.Enqueue<IExchangeRateProviderStreamRateService>(socket =>
            socket.StartStreamAsync(tokenRequest, null, true, cancellation));

        logger.LogInformation("Enqueued new CryptoCompare WebSocket background job: {JobId}", id);
    }

    private void RegisterReconnectJob(CancellationToken cancellationToken)
    {
        /// Attention
        /// ===================================================================
        ///  YOU HAVE TO GET YOUR CREDENTIALS FROM YOUR OWN DATABASE OR VAULT AND THEN SET IT HERE.
        /// ===================================================================

        const string jobId = "ReconnectExchangeRateStream";

        var expireIn = options.TokenExpireInSec * 0.10;
        var interval = (options.TokenExpireInSec - expireIn) / 60;

        recurringJobManager.AddOrUpdate<IExchangeRateProviderStreamRateService>(
            jobId,
            x => x.ReconnectAsync(new GetTokenRequest()
            {
                ClientId = "ZzhFkoKfr4o_GbiKTqMduob1xhOQeSnXfhrf2bq1lJBMmocAiOgWAC3B8M6hN305",
                ClientSecret = "xV7akVEkpG8n0NO4prjK8qBtPFfIlBGLktHmUi19g57eGVl470RY1cDFu1AmRjGbjcMR31uq/TGXuW4PNgy4nQ==",
                Scopes = "realtime-api"
            }, cancellationToken),
            $"*/{interval} * * * *");

        _jobIds.TryAdd("ExchangeRate", jobId);
        logger.LogInformation("Registered recurring job for reconnecting WebSocket stream: {JobId}", jobId);
    }

    private void RemoveReconnectJob()
    {
        if (_jobIds.TryRemove("ExchangeRate", out string jobId))
        {
            recurringJobManager.RemoveIfExists(jobId);
            logger.LogInformation("Removed recurring reconnect job: {JobId}", jobId);
        }
    }
}
