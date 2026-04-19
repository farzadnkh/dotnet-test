using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NT.KYC.Jibit.HashiCorpVault.Extensions;
using NT.KYC.Jibit.RestClients;
using NT.KYC.Jibit.RestClients.APIs.AccessToken;
using NT.KYC.Jibit.RestClients.APIs.BiometricKyc;
using NT.KYC.Jibit.RestClients.APIs.Cards;
using NT.KYC.Jibit.RestClients.APIs.DocumentKyc;
using NT.KYC.Jibit.RestClients.APIs.IdentityDetail;
using NT.KYC.Jibit.RestClients.APIs.Matching;
using NT.KYC.Jibit.Utils;
using NT.SDK.RestClient.Models;

namespace NT.KYC.Jibit.Bootstrapper;

public static class JibitClientBootstrapper
{
    public static IServiceCollection AddJibitRestClient(this IServiceCollection serviceCollection, JibitRestClientConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDebugMode())
        {
            serviceCollection.AddSingleton<IJibitAccessTokenApi, FakeJibitAccessTokenApi>();
            serviceCollection.AddSingleton<IIdentityDetailApi, FakeIdentityDetailApi>();
            serviceCollection.AddSingleton<IMatchingApi, FakeMatchingApi>();
            serviceCollection.AddSingleton<ICardsApi, FakeCardsApi>();
        }
        else
        {
            serviceCollection.AddSingleton<IJibitAccessTokenApi, JibitAccessTokenApi>();
            serviceCollection.AddSingleton<IIdentityDetailApi, IdentityDetailApi>();
            serviceCollection.AddSingleton<IMatchingApi, MatchingApi>();
            serviceCollection.AddSingleton<ICardsApi, CardsApi>();
        }

        serviceCollection.AddSingleton<IJibitRestClient, JibitRestClient>();
        serviceCollection.AddSingleton(configuration);

        return serviceCollection;
    }

    public static IServiceCollection AddJibitAlphaRestClient(this IServiceCollection serviceCollection, JibitAlphaRestClientConfiguration configuration, IWebHostEnvironment environment)
    {
        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddSingleton<IJibitAlphaRestClient, JibitAlphaRestClient>();

        if (environment.IsDebugMode())
        {
            serviceCollection.AddSingleton<IDocumentKycApi, FakeDocumentKycApi>();
            serviceCollection.AddSingleton<IBiometricKycApi, FakeBiometricKycApi>();
        }
        else
        {
            serviceCollection.AddSingleton<IDocumentKycApi, DocumentKycApi>();
            serviceCollection.AddSingleton<IBiometricKycApi, BiometricKycApi>();
        }

        return serviceCollection;
    }
}