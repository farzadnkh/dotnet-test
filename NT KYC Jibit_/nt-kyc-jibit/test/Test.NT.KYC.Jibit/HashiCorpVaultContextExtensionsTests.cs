using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NT.HashiCorp.Vault.Abstraction;
using NT.KYC.Jibit.HashiCorpVault.Extensions;
using NT.KYC.Jibit.RestClients;
using NT.KYC.Jibit.RestClients.APIs.AccessToken;
using NT.KYC.Jibit.RestClients.APIs.BiometricKyc;
using NT.KYC.Jibit.RestClients.APIs.DocumentKyc;
using NT.KYC.Jibit.Utils;

namespace Test.NT.KYC.Jibit;

public class HashiCorpVaultContextExtensionsTests
{
    [Fact]
    public async Task AddJibitClientAsync_LoadsCredentialsAndRegistersClients()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var credentialsPath = Path.Combine(tempDirectory, "vault.credentials.json");

        await File.WriteAllTextAsync(credentialsPath,
        """
        {
          "credentials": {
            "jibit_connections": {
              "ApiKey": "api-from-vault",
              "SecretKey": "secret-from-vault",
              "PermanentToken": "alpha-permanent"
            }
          }
        }
        """);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = tempDirectory
        });
        builder.Services.AddLogging();
        builder.Environment.EnvironmentName = "Debug";

        try
        {
            await builder.AddHashiCorpVaultAsync(
                options =>
                {
                    options.JsonFileName = Path.GetFileName(credentialsPath);
                    options.EnvironmentFileName = ".env";
                },
                async (context, cancellationToken) =>
                {
                    await context.AddJibitClientAsync(
                        builder.Environment,
                        opt =>
                        {
                            opt.Logger = _ => { };
                            opt.JibitBasePath = "https://jibit.base";
                            opt.JibitAlphaBasePath = "https://alpha.base";
                            opt.Timeout = 6000;
                        },
                        cancellationToken: cancellationToken);
                },
                isDebugMode: true);

            using var provider = builder.Services.BuildServiceProvider();
            var restClient = provider.GetRequiredService<IJibitRestClient>();
            var alphaClient = provider.GetRequiredService<IJibitAlphaRestClient>();

            Assert.Equal("api-from-vault", restClient.JibitConfiguration.ApiKey);
            Assert.Equal("secret-from-vault", restClient.JibitConfiguration.Password);
            Assert.Equal("https://jibit.base", restClient.JibitConfiguration.BasePath);
            Assert.Equal(6000, restClient.JibitConfiguration.Timeout);

            Assert.Equal("alpha-permanent", alphaClient.JibitAlphaConfiguration.PermanentToken);
            Assert.Equal("https://alpha.base", alphaClient.JibitAlphaConfiguration.BasePath);

            Assert.IsType<FakeJibitAccessTokenApi>(provider.GetRequiredService<IJibitAccessTokenApi>());
            Assert.IsType<FakeDocumentKycApi>(provider.GetRequiredService<IDocumentKycApi>());
            Assert.IsType<FakeBiometricKycApi>(provider.GetRequiredService<IBiometricKycApi>());
        }
        finally
        {
            try
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
            catch
            {
                // ignore cleanup failures
            }
        }
    }
}
