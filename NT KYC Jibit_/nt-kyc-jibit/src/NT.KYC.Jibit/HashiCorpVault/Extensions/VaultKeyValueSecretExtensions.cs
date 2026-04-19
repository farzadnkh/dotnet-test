using NT.HashiCorp.Vault.Abstraction;
using NT.KYC.Jibit.HashiCorpVault.Models;

namespace NT.KYC.Jibit.HashiCorpVault.Extensions;

public static class VaultKeyValueSecretExtensions
{
    public static async Task<JibitClientCredentials> ReadJibitClientSecretAsync(this HashiCorpVaultContext context,
        string connectionKey, string credentialsMountPoint, CancellationToken cancellationToken = default)
    {
        if (context.IsDebugMode)
            return context.GetCredentials<JibitClientCredentials>(connectionKey, credentialsMountPoint);

        return await context.Secrets.ReadSecretAsync<JibitClientCredentials>(connectionKey, credentialsMountPoint,
            cancellationToken);
    }
}