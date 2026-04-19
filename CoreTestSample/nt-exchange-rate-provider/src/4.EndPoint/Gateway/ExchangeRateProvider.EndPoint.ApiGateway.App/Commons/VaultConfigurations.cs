using ExchangeRateProvider.EndPoint.ApiGateway.App.Commons.Extensions;
using NT.HashiCorp.Vault.Abstraction;

namespace ExchangeRateProvider.EndPoint.ApiGateway.App.Commons;

public static class VaultConfigurations
{
    internal static async Task<WebApplicationBuilder> ConfigureVaultServerAsync(this WebApplicationBuilder builder)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        await builder.AddHashiCorpVaultAsync(option =>
        {
            option.EnvironmentFileName = ".env";
            option.JsonFileName = "vault.credentials.json";
            option.VaultServiceTimeout = TimeSpan.FromSeconds(60);
        },
        HachiCorpVaultImplementationsAsync, isDebugMode: environment.Equals("Debug", StringComparison.CurrentCultureIgnoreCase),
            cancellationToken: default);

        return builder;
    }

    private static async Task HachiCorpVaultImplementationsAsync(IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.HealthCheckAsync(cancellationToken: cancellationToken);

        var reversProxy = await context.AddYarpAsync(pathAction: path =>
        {
            context.Configurations.GetSection("VaultOption:ReverseProxy").Bind(path);
        }, cancellationToken);
    }
}
