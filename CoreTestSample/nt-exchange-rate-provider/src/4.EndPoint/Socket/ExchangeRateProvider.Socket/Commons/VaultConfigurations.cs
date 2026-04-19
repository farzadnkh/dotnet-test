using ExchangeRateProvider.Infrastructure.Sql.Commons;
using NT.HashiCorp.Vault.Abstraction;

namespace ExchangeRateProvider.Socket.Commons;

public static class VaultConfigurations
{
    public static async Task<WebApplicationBuilder> ConfigureVaultServerAsync(this WebApplicationBuilder builder)
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
        await context.AddInfrastructuresAsync(cancellationToken);
    }
} 