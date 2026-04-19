using ExchangeRateProvider.EndPoint.ApiGateway.App.Models;
using NT.HashiCorp.Vault.Abstraction;

namespace ExchangeRateProvider.EndPoint.ApiGateway.App.Commons.Extensions;

internal static partial class VaultKeyValueSecretsExtensions
{
    internal static async Task<YarpCredentials> ReadYarpConnectionSecretAsync(this HashiCorpVaultContext context,
        string yarpConnectionKey, string credentialsMountPoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(yarpConnectionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialsMountPoint);

        if (context.IsDebugMode)
            return context.GetCredentials<YarpCredentials>(yarpConnectionKey, credentialsMountPoint);

        var secret = await context.Secrets.ReadSecretAsync<YarpCredentials>(
            key: yarpConnectionKey,
            credentialsMountPoint: credentialsMountPoint,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        HashiCorpVaultSecretNotFoundException.ThrowIfNull(secret, yarpConnectionKey, credentialsMountPoint);
        HashiCorpVaultException.ThrowIfNotValid(secret);

        return secret;
    }
}