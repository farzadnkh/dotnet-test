using Microsoft.Extensions.Logging;
using NT.HashiCorp.Vault.Abstraction;
using NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Options;
using NT.SDK.ExchangeRateProvider.Bootstrappers;
using NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Extensions;

namespace NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Extensions
{
    public static class HashiCorpVaultContextExtensions
    {
        public static async Task<IHashiCorpVaultContext> AddExchangeRateProvider(
            this IHashiCorpVaultContext context,
            Action<HashiCorpVaultPaths> pathAction = null,
            Action<ExrpOptions> option = null,
            CancellationToken cancellationToken = default)
        {
            var logger = BootstrapLogger.BootstrapLogger.CreateLogger(typeof(HashiCorpVaultContextExtensions).FullName ?? "HashiCorpVaultContextExtensions:AddExchangeRateProvider");
            var requestId = Guid.NewGuid().ToString();
            using (logger.BeginScope("Processing ExchangeRateProvider integration. RequestId: {RequestId}", requestId))
            {
                logger.LogInformation("Starting AddExchangeRateProvider process.");

                cancellationToken.ThrowIfCancellationRequested();

                ArgumentNullException.ThrowIfNull(context);

                var paths = new HashiCorpVaultPaths(
                    connectionKey: HashiCorpVaultOptionDefaults.ConnectionKey,
                    credentialsMountPoint: HashiCorpVaultOptionDefaults.CredentialsMountPoint
                );

                var options = new ExrpOptions();

                pathAction?.Invoke(paths);
                option?.Invoke(options);

                if (context is not HashiCorpVaultContext hashiCorpVaultContext)
                {
                    logger.LogError("Invalid context type. Expected HashiCorpVaultContext but received {ContextType}.", context.GetType());
                    throw new InvalidOperationException("The provided context is not of type HashiCorpVaultContext.");
                }

                logger.LogInformation("Context validation passed.");

                if (hashiCorpVaultContext is null)
                {
                    logger.LogError("HashiCorpVaultContext is null.");
                    throw new NullReferenceException(nameof(hashiCorpVaultContext));
                }

                logger.LogInformation("Fetching ExchangeRateProvider secret from Vault...");

                var secret = await hashiCorpVaultContext.ReadExchangeRateProviderConnectionSecretAsync(
                    exrpConnectionKey: paths.ConnectionKey,
                    credentialsMountPoint: paths.CredentialsMountPoint,
                    cancellationToken: cancellationToken);

                HashiCorpVaultSecretNotFoundException.ThrowIfNull(secret, paths.ConnectionKey, paths.CredentialsMountPoint);
                HashiCorpVaultException.ThrowIfNotValid(secret);

                logger.LogInformation("Successfully retrieved ExchangeRateProvider credentials. Configuring ExchangeRateProvider connection...");

                hashiCorpVaultContext.Services.AddExchangeRateProvider(o =>
                {
                    o.BasePath = secret.BasePath;
                    o.CachePrefix = options.CachePrefix;
                    o.TokenExpireInSec = options.TokenExpireInSec;
                });
                logger.LogInformation("Successfully completed ExchangeRateProvider integration.");
            }

            return context;
        }
    }
}

