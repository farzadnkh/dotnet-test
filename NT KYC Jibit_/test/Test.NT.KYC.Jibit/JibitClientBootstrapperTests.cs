using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NT.KYC.Jibit.Bootstrapper;
using NT.KYC.Jibit.HashiCorpVault.Extensions;
using NT.KYC.Jibit.RestClients;
using NT.KYC.Jibit.RestClients.APIs.AccessToken;
using NT.KYC.Jibit.RestClients.APIs.BiometricKyc;
using NT.KYC.Jibit.RestClients.APIs.Cards;
using NT.KYC.Jibit.RestClients.APIs.DocumentKyc;
using NT.KYC.Jibit.RestClients.APIs.IdentityDetail;
using NT.KYC.Jibit.RestClients.APIs.Matching;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Logger;

namespace Test.NT.KYC.Jibit;

public class JibitClientBootstrapperTests
{
    private static JibitRestClientConfiguration CreateRestConfig() =>
        new("api-key", "secret", "https://api.test", 3000, _ => { });

    private static JibitAlphaRestClientConfiguration CreateAlphaConfig() =>
        new("permanent-token", "https://alpha.test", 3000, _ => { });

    [Fact]
    public void AddJibitRestClient_InDebugMode_RegistersFakeApis()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns("Debug");

        services.AddJibitRestClient(CreateRestConfig(), environment.Object);
        services.AddJibitAlphaRestClient(CreateAlphaConfig(), environment.Object);

        using var provider = services.BuildServiceProvider();

        Assert.IsType<FakeJibitAccessTokenApi>(provider.GetRequiredService<IJibitAccessTokenApi>());
        Assert.IsType<FakeIdentityDetailApi>(provider.GetRequiredService<IIdentityDetailApi>());
        Assert.IsType<FakeMatchingApi>(provider.GetRequiredService<IMatchingApi>());
        Assert.IsType<FakeCardsApi>(provider.GetRequiredService<ICardsApi>());
        Assert.IsType<FakeDocumentKycApi>(provider.GetRequiredService<IDocumentKycApi>());
        Assert.IsType<FakeBiometricKycApi>(provider.GetRequiredService<IBiometricKycApi>());

        var restClient = provider.GetRequiredService<IJibitRestClient>();
        Assert.Equal("api-key", restClient.JibitConfiguration.ApiKey);

        var alphaClient = provider.GetRequiredService<IJibitAlphaRestClient>();
        Assert.Equal("permanent-token", alphaClient.JibitAlphaConfiguration.PermanentToken);
    }

    [Fact]
    public void AddJibitRestClient_InProduction_RegistersRealApis()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns("Production");

        services.AddJibitRestClient(CreateRestConfig(), environment.Object);
        services.AddJibitAlphaRestClient(CreateAlphaConfig(), environment.Object);

        using var provider = services.BuildServiceProvider();

        Assert.IsType<JibitAccessTokenApi>(provider.GetRequiredService<IJibitAccessTokenApi>());
        Assert.IsType<IdentityDetailApi>(provider.GetRequiredService<IIdentityDetailApi>());
        Assert.IsType<MatchingApi>(provider.GetRequiredService<IMatchingApi>());
        Assert.IsType<CardsApi>(provider.GetRequiredService<ICardsApi>());
        Assert.IsType<DocumentKycApi>(provider.GetRequiredService<IDocumentKycApi>());
        Assert.IsType<BiometricKycApi>(provider.GetRequiredService<IBiometricKycApi>());
    }
}
