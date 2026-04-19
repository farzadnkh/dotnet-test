using Microsoft.AspNetCore.Hosting;
using NT.HashiCorp.Vault.Abstraction;
using NT.KYC.Jibit.Bootstrapper;
using NT.KYC.Jibit.HashiCorpVault.Models;
using NT.KYC.Jibit.HashiCorpVault.Options;
using NT.KYC.Jibit.Utils;

namespace NT.KYC.Jibit.HashiCorpVault.Extensions;

public static class HashiCorpVaultContextExtensions
{
    public static async Task<IHashiCorpVaultContext> AddJibitClientAsync(
        this IHashiCorpVaultContext context,
        IWebHostEnvironment environment,
        Action<JibitOptions> optionAction,
        Action<HashiCorpVaultPaths>? pathAction = null,
        CancellationToken cancellationToken = default
    )
    {
        var paths = PrepareHashiCorpVaultPath(context, optionAction, pathAction, cancellationToken);

        var hashiCorpVaultContext = context as HashiCorpVaultContext;

        ArgumentNullException.ThrowIfNull(hashiCorpVaultContext);

        var secret = await GetJibitCredentialsFromVault(paths, hashiCorpVaultContext, cancellationToken);

        var options = InvokeJibitOptions(optionAction);

        var jibitRestClientConfiguration = PrepareConfigurationBasedOnOptions(secret, options);

        var jibitRestAlphaClientConfiguration = PrepareJibitAlphaConfigurationBasedOnOptions(secret, options);

        context.Services.AddJibitRestClient(jibitRestClientConfiguration, environment);
        context.Services.AddJibitAlphaRestClient(jibitRestAlphaClientConfiguration, environment);

        return context;
    }

    private static HashiCorpVaultPaths PrepareHashiCorpVaultPath(IHashiCorpVaultContext context,
        Action<JibitOptions> optionAction, Action<HashiCorpVaultPaths>? pathAction, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(optionAction);

        var paths = new HashiCorpVaultPaths(
            HashiCorpVaultDefaultOptions.JibitClientConnectionKey,
            HashiCorpVaultDefaultOptions.CredentialsMountPoint
        );

        pathAction?.Invoke(paths);
        return paths;
    }

    private static async Task<JibitClientCredentials> GetJibitCredentialsFromVault(HashiCorpVaultPaths paths,
        HashiCorpVaultContext hashiCorpVaultContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(hashiCorpVaultContext);

        var secret = await hashiCorpVaultContext.ReadJibitClientSecretAsync(
            paths.ConnectionKey,
            paths.CredentialsMountPoint,
            cancellationToken
        );

        HashiCorpVaultSecretNotFoundException.ThrowIfNull(
            secret,
            paths.ConnectionKey,
            paths.CredentialsMountPoint
        );
        HashiCorpVaultException.ThrowIfNotValid(secret);
        return secret;
    }

    private static JibitRestClientConfiguration PrepareConfigurationBasedOnOptions(JibitClientCredentials secret,
        JibitOptions options)
    {
        var jibitConfiguration = new JibitRestClientConfiguration(
            secret.ApiKey,
            secret.SecretKey,
            options.JibitBasePath,
            options.Timeout,
            options.Logger
        )
        {
            Proxy = options.Proxy,
            DefaultHeaders = options.DefaultHeaders,
            TempFolderPath = options.TempFolderPath,
            ClientCertificates = options.ClientCertificates,
            DateTimeFormat = options.DateTimeFormat
        };

        return jibitConfiguration;
    }

    private static JibitAlphaRestClientConfiguration PrepareJibitAlphaConfigurationBasedOnOptions(
        JibitClientCredentials secret, JibitOptions options)
    {
        var jibitConfiguration = new JibitAlphaRestClientConfiguration(
            secret.PermanentToken,
            options.JibitAlphaBasePath,
            options.Timeout,
            options.Logger
        )
        {
            Proxy = options.Proxy,
            DefaultHeaders = options.DefaultHeaders,
            TempFolderPath = options.TempFolderPath,
            ClientCertificates = options.ClientCertificates,
            DateTimeFormat = options.DateTimeFormat
        };

        return jibitConfiguration;
    }

    private static JibitOptions InvokeJibitOptions(Action<JibitOptions> optionAction)
    {
        var options = new JibitOptions();
        optionAction.Invoke(options);

        ArgumentNullException.ThrowIfNull(options.Logger);
        ArgumentNullException.ThrowIfNull(options);
        return options;
    }
}