using ExchangeRateProvider.Adapter.CryptoCompare.Services;
using ExchangeRateProvider.Adapter.Xe.Services;
using ExchangeRateProvider.Application.Settings.Handlers.Admin;
using ExchangeRateProvider.Contract.Settings;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Enums;
using Hangfire;
using System.Threading.Channels;

namespace ExchangeRateProvider.Admin.Workers
{
    public class JobManagement(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IServiceScopeFactory serviceScopeFactory,
        IChannelRegistry channelRegistry,
        ISettingQueryRepository settingQueryRepository,
        ILogger<JobManagement> logger) : IHostedLifecycleService
    {
        #region Props

        private bool _shutdownInitiated = false;
        private bool _channelCompleted = false;
        private Timer _shutdownTimer;
        private readonly ChannelReader<BackgroundJobSyncMessageArgs> _backgroundJobSyncReader = channelRegistry.GetReader<BackgroundJobSyncMessageArgs>();
        private readonly ChannelReader<JobsIntervalMessageArgs> _jobsIntervalReader = channelRegistry.GetReader<JobsIntervalMessageArgs>();

        private Dictionary<ProviderType, string> _jobIds = [];

        private List<Task> _messagesHandlerTask = [];

        #endregion

        #region Start Section

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("JobManagement StartAsync completed.");
            return Task.CompletedTask;
        }

        public async Task StartingAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                logger.LogCritical("Initiating a 1-minute wait for publishers to complete their tasks due to shutdown signal.");
                _shutdownInitiated = true;
                _shutdownTimer = new Timer(_ => ForceCompleteChannel(), null, TimeSpan.FromSeconds(65), Timeout.InfiniteTimeSpan);
            });

            try
            {
                var settings = await SettingHandler.GetSetting(settingQueryRepository, default);
                JobsIntervalMessageArgs jobsInterval = new()
                {
                    CryptoJobInterval = settings.CryptoJobsInterval,
                    FiatsJobInterval = settings.FiatJobsInterval
                };

                IEnumerable<ExchangeRateProviderApiAccount> providerApiAccounts = await GetAllPublished(cancellationToken);
                RegisterCryptoCompareWorkers(providerApiAccounts, jobsInterval, cancellationToken);
                RegisterXeWorkers(providerApiAccounts, jobsInterval, cancellationToken);
                logger.LogInformation("Initial CryptoCompare workers registered in StartingAsync.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to initialize job management during startup.");
                throw;
            }
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            _messagesHandlerTask.Add(HandleJobsIntervalSyncMessageAsync(cancellationToken));
            _messagesHandlerTask.Add(HandleBackgroundJobSyncMessageAsync(cancellationToken));

            logger.LogInformation("JobManagement StartedAsync completed. Channel handlers started in background.");

            return Task.CompletedTask;
        }

        #endregion

        #region Stop Section

        public async Task StoppingAsync(CancellationToken cancellationToken)
        {
            logger.LogCritical("Application is shutting down. Attempting graceful channel completion.");
            if (!_channelCompleted)
            {
                _channelCompleted = channelRegistry.TryComplete<BackgroundJobSyncMessageArgs>();
                channelRegistry.TryComplete<BackgroundJobSyncMessageArgs>();
            }

            try
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                await Task.WhenAny(Task.WhenAll(_messagesHandlerTask), timeoutTask);

                if (!timeoutTask.IsCompleted)
                {
                    logger.LogInformation("Channel handlers completed gracefully during stopping.");
                }
                else
                {
                    logger.LogWarning("Channel handlers did not complete within the graceful shutdown timeout. Some messages might be unprocessed.");
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Graceful channel shutdown was cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during graceful channel shutdown attempt.");
            }

            _shutdownTimer?.Dispose();
            logger.LogInformation("JobManagement StoppingAsync completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Cleaning up Hangfire server in StopAsync.");
            return Task.CompletedTask;
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("JobManagement has completely stopped.");
            return Task.CompletedTask;
        }

        #endregion

        #region Channel Handler

        private async Task HandleJobsIntervalSyncMessageAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting Jobs Interval message handler.");
            try
            {
                await foreach (var message in _jobsIntervalReader.ReadAllAsync(cancellationToken))
                {
                    var account = await GetAllPublished(cancellationToken);
                    logger.LogDebug("Received ProviderSync message for provider: {IntervalType}", message.IntervalTypes);

                    logger.LogInformation("Re-registering workers for synced provider: {IntervalType}", message.IntervalTypes);
                    switch (message.IntervalTypes)
                    {
                        case IntervalTypes.Crypto:
                            RegisterCryptoCompareWorkers(account, message, cancellationToken);
                            break;
                        case IntervalTypes.Fiats:
                            RegisterXeWorkers(account, message, cancellationToken);
                            break;
                        case IntervalTypes.All:
                            RegisterCryptoCompareWorkers(account, message, cancellationToken);
                            RegisterXeWorkers(account, message, cancellationToken);
                            break;
                        default:
                            break;
                    }
                }
                logger.LogInformation("Jobs Interval message channel reader completed.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Jobs Interval message handler was cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling Jobs Interval messages. Channel will stop reading.");
            }
        }

        private async Task HandleBackgroundJobSyncMessageAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting BackgroundJobSync message handler.");
            try
            {
                await foreach (var message in _backgroundJobSyncReader.ReadAllAsync(cancellationToken))
                {
                    var account = await GetAllPublished(cancellationToken);
                    logger.LogDebug("Received ProviderSync message for provider: {ProviderType}", message.ProviderType);

                    logger.LogInformation("Re-registering workers for synced provider: {ProviderType}", message.ProviderType);
                    switch (message.ProviderType)
                    {
                        case ProviderType.CryptoCompare:
                            RegisterCryptoCompareWorkers(account, new(), cancellationToken);
                            break;
                        case ProviderType.XE:
                            RegisterXeWorkers(account, new(), cancellationToken);
                            break;
                        default:
                            break;
                    }
                }
                logger.LogInformation("BackgroundJobSync message channel reader completed.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "BackgroundJobSync message handler was cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling BackgroundJobSync messages. Channel will stop reading.");
            }
        }
        #endregion

        #region Providers Registry

        private void RegisterCryptoCompareWorkers(IEnumerable<ExchangeRateProviderApiAccount> providerAccounts, JobsIntervalMessageArgs jobsIntervalMessageArgs, CancellationToken cancellationToken)
        {
            if (providerAccounts == null)
            {
                logger.LogWarning("Attempted to register CryptoCompare workers with a null providerAccounts collection. Skipping registration.");
                return;
            }

            var cryptoCompareAccount = providerAccounts
                .FirstOrDefault(acc => acc.ExchangeRateProvider?.Type == ProviderType.CryptoCompare);

            if (cryptoCompareAccount is null || cryptoCompareAccount.ExchangeRateProvider?.Type != ProviderType.CryptoCompare)
            {
                logger.LogWarning("""
                CryptoCompare API account is missing or unpublished.
                The worker processes for CryptoCompare will not be started or will be removed.
                """);

                if (_jobIds.TryGetValue(ProviderType.CryptoCompare, out string existingJobId) && !string.IsNullOrEmpty(existingJobId))
                {
                    if (existingJobId.StartsWith("recurring:"))
                    {
                        recurringJobManager.RemoveIfExists(existingJobId);
                        logger.LogInformation("Removed existing CryptoCompare recurring job: {JobId}", existingJobId);
                    }
                    else
                    {
                        backgroundJobClient.Delete(existingJobId);
                        logger.LogInformation("Deleted existing CryptoCompare background job: {JobId}", existingJobId);
                    }
                    _jobIds.Remove(ProviderType.CryptoCompare);
                }
                return;
            }

            _jobIds.TryGetValue(ProviderType.CryptoCompare, out string currentJobId);

            if (cryptoCompareAccount.ProtocolType is ProtocolType.WebSocket)
            {
                if (!string.IsNullOrEmpty(currentJobId))
                {
                    if (currentJobId.StartsWith("recurring:"))
                    {
                        recurringJobManager.RemoveIfExists(currentJobId);
                        logger.LogInformation("Removed old CryptoCompare recurring job before starting new WebSocket job: {JobId}", currentJobId);
                    }
                    else
                    {
                        backgroundJobClient.Delete(currentJobId);
                        logger.LogInformation("Deleted old CryptoCompare background job before starting new WebSocket job: {JobId}", currentJobId);
                    }
                    _jobIds.Remove(ProviderType.CryptoCompare);
                }

                if (!(_jobIds.ContainsKey(ProviderType.CryptoCompare) && _jobIds[ProviderType.CryptoCompare].StartsWith("job:")))
                {
                    var id = backgroundJobClient.Enqueue<SocketRateService>(provider =>
                        provider.StreamRates(cryptoCompareAccount.Id, cancellationToken));
                    _jobIds[ProviderType.CryptoCompare] = id;
                    logger.LogInformation("Enqueued new CryptoCompare WebSocket background job: {JobId}", id);
                }
                else
                {
                    logger.LogInformation("CryptoCompare WebSocket job is already running or registered with ID: {JobId}", _jobIds[ProviderType.CryptoCompare]);
                }
            }
            else if (cryptoCompareAccount.ProtocolType is ProtocolType.ApiCall)
            {
                if (!string.IsNullOrEmpty(currentJobId))
                {
                    if (!currentJobId.StartsWith("recurring:"))
                    {
                        backgroundJobClient.Delete(currentJobId);
                        logger.LogInformation("Deleted old CryptoCompare background job before starting new API Call job: {JobId}", currentJobId);
                    }
                    else
                    {
                        logger.LogInformation("Existing recurring job will be updated. JobId: {JobId}", currentJobId);
                    }
                    _jobIds.Remove(ProviderType.CryptoCompare);
                }

                string recurringJobName = "CryptoCompareApiRateProvider";
                recurringJobManager.AddOrUpdate<ApiRateServices>(
                    recurringJobName,
                    provider => provider.StreamRates(cryptoCompareAccount.Id, cancellationToken),
                    $"*/{jobsIntervalMessageArgs.CryptoJobInterval} * * * *");

                _jobIds[ProviderType.CryptoCompare] = $"recurring:{recurringJobName}"; // Store a distinct identifier for recurring jobs
                logger.LogInformation("Added or updated CryptoCompare API Call recurring job: {JobName}", recurringJobName);
            }
            else
            {
                logger.LogWarning("CryptoCompare API account has an unsupported protocol type: {ProtocolType}. No workers will be started for it.", cryptoCompareAccount.ProtocolType);
                if (_jobIds.TryGetValue(ProviderType.CryptoCompare, out string existingJobId) && !string.IsNullOrEmpty(existingJobId))
                {
                    if (existingJobId.StartsWith("recurring:"))
                    {
                        recurringJobManager.RemoveIfExists(existingJobId);
                        logger.LogInformation("Removed existing CryptoCompare recurring job due to unsupported protocol type: {JobId}", existingJobId);
                    }
                    else
                    {
                        backgroundJobClient.Delete(existingJobId);
                        logger.LogInformation("Deleted existing CryptoCompare background job due to unsupported protocol type: {JobId}", existingJobId);
                    }
                    _jobIds.Remove(ProviderType.CryptoCompare);
                }
            }
        }

        private void RegisterXeWorkers(IEnumerable<ExchangeRateProviderApiAccount> providerAccounts, JobsIntervalMessageArgs jobsIntervalMessageArgs, CancellationToken cancellationToken)
        {
            if (providerAccounts == null)
            {
                logger.LogWarning("Attempted to register Xe workers with a null providerAccounts collection. Skipping registration.");
                return;
            }

            var XeAccount = providerAccounts
                .FirstOrDefault(acc => acc.ExchangeRateProvider?.Type == ProviderType.XE);

            string recurringJobName = "XeApiRateServices";

            if (XeAccount is null || XeAccount.ExchangeRateProvider?.Type != ProviderType.XE || !XeAccount.Published)
            {
                logger.LogWarning("""
                    Xe API account is missing or unpublished.
                    The worker processes for Xe will not be started or will be removed.
                    """);

                recurringJobManager.RemoveIfExists(recurringJobName);
                return;
            }

            if (XeAccount.ProtocolType is ProtocolType.WebSocket)
            {
                logger.LogError("""
                    Protocol Websocket is not Registerable for Xe Provider. 
                    The Ex Worker wont start. From admin Panel Try To change The Xe Provider Protocol
                    """);

                return;
            }

            recurringJobManager.AddOrUpdate<XeApiRateServices>(
                recurringJobName,
                provider => provider.StreamRates(XeAccount.Id, cancellationToken),
                $"*/{jobsIntervalMessageArgs.FiatsJobInterval} * * * *");

            logger.LogInformation("Added or updated Xe API Call recurring job: {JobName}", recurringJobName);
        }
        #endregion

        #region Utilities

        private void ForceCompleteChannel()
        {
            if (!_channelCompleted && _shutdownInitiated)
            {
                logger.LogCritical("1-minute wait completed. Forcing completion of CryptoCompare message channel.");
                _channelCompleted = channelRegistry.TryComplete<BackgroundJobSyncMessageArgs>();

                if (_backgroundJobSyncReader.Count > 0)
                {
                    logger.LogCritical("CryptoCompare channel was marked as completed, but {MessageCount} messages remain unread. We have time till the host stops.", _backgroundJobSyncReader.Count);
                }
            }
            _shutdownTimer?.Dispose();
            _shutdownTimer = null;
        }

        private async Task<IEnumerable<ExchangeRateProviderApiAccount>> GetAllPublished(CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IProviderApiAccountQueryRepository>();

            try
            {
                var providerAccounts = await repository.GetAllPublishProvidersWithAllIncludesAsync(cancellationToken);

                if (providerAccounts == null || !providerAccounts.Any())
                {
                    logger.LogWarning("No published provider accounts found. Returning an empty collection.");
                    return [];
                }

                logger.LogInformation("Successfully retrieved {Count} published provider accounts.", providerAccounts.Count());
                return providerAccounts;
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Operation to retrieve provider accounts was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An unexpected error occurred while retrieving published provider accounts: {ErrorMessage}", ex.Message);
                throw;
            }
        }
        #endregion
    }
}