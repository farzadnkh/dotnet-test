using NT.HashiCorp.Vault.Abstraction;
using NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Options;

namespace NT.SDK.ExchangeRateProvider.HashiCorp.Vault.Extensions;

internal static partial class VaultKeyValueSecretsExtensions
{
    internal static async Task<ExrpCredentials> ReadExchangeRateProviderConnectionSecretAsync(this HashiCorpVaultContext context,
        string exrpConnectionKey, string credentialsMountPoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(exrpConnectionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialsMountPoint);

        if (context.IsDebugMode)
            return context.GetCredentials<ExrpCredentials>(exrpConnectionKey, credentialsMountPoint);

        var secret = await context.Secrets.ReadSecretAsync<ExrpCredentials>(
            key: exrpConnectionKey,
            credentialsMountPoint: credentialsMountPoint,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        HashiCorpVaultSecretNotFoundException.ThrowIfNull(secret, exrpConnectionKey, credentialsMountPoint);
        HashiCorpVaultException.ThrowIfNotValid(secret);

        return secret;
    }
}