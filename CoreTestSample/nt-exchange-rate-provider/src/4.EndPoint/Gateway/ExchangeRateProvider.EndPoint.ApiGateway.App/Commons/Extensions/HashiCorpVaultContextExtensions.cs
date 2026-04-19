using ExchangeRateProvider.EndPoint.ApiGateway.App.Options;
using NT.BootstrapLogger;
using NT.HashiCorp.Vault.Abstraction;

namespace ExchangeRateProvider.EndPoint.ApiGateway.App.Commons.Extensions
{
    public static class HashiCorpVaultContextExtensions
    {
        public static async Task<IHashiCorpVaultContext> AddYarpAsync(
            this IHashiCorpVaultContext context,
            Action<HashiCorpVaultPaths> pathAction = null,
            CancellationToken cancellationToken = default)
        {
            var logger = BootstrapLogger.CreateLogger(typeof(HashiCorpVaultContextExtensions).FullName ?? "HashiCorpVaultContextExtensions:AddYarpAsync");
            var requestId = Guid.NewGuid().ToString();
            using (logger.BeginScope("Processing Yarp integration. RequestId: {RequestId}", requestId))
            {
                logger.LogInformation("Starting AddYarpAsync process.");

                cancellationToken.ThrowIfCancellationRequested();

                ArgumentNullException.ThrowIfNull(context);

                var paths = new HashiCorpVaultPaths(
                    connectionKey: HashiCorpVaultOptionDefaults.YarpConnectionKey,
                    credentialsMountPoint: HashiCorpVaultOptionDefaults.CredentialsMountPoint
                );

                pathAction?.Invoke(paths);

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

                logger.LogInformation("Fetching Yarp secret from Vault...");

                var secret = await hashiCorpVaultContext.ReadYarpConnectionSecretAsync(
                    yarpConnectionKey: paths.ConnectionKey,
                    credentialsMountPoint: paths.CredentialsMountPoint,
                    cancellationToken: cancellationToken);

                HashiCorpVaultSecretNotFoundException.ThrowIfNull(secret, paths.ConnectionKey, paths.CredentialsMountPoint);
                HashiCorpVaultException.ThrowIfNotValid(secret);

                logger.LogInformation("Successfully retrieved Yarp credentials. Configuring Yarp connection...");

                hashiCorpVaultContext.Services.AddReverseProxy().LoadFromMemory(secret.Routes, secret.Clusters);
                logger.LogInformation("Successfully completed Yarp integration.");
            }

            return context;
        }
    }
}

