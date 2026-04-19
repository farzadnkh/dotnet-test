using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace ExchangeRateProvider.Api.Seeders;

public static class Config
{
    public static IEnumerable<IdentityResource> GetIdentityResources() =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        ];

    public static IEnumerable<ApiScope> GetApiScopes() =>
        [
            new ApiScope("api1.read", "Read access to API 1"),
            new ApiScope("api1.write", "Write access to API 1")
        ];

    public static IEnumerable<ApiResource> GetApiResources() =>
        [
            new("cpl", "Admin Panel")
            {
                Scopes = { "cpl", "role" },
            }
        ];

    public static IEnumerable<Client> GetClients() =>
        [
            new()
            {
                ClientId = "Front-ClientId",
                ClientName = "Front Client App",
                ClientSecrets = { new Secret("super-secret-key".Sha256()) },
                AllowedGrantTypes = GrantTypes.Hybrid,
                RequirePkce = true,
                RequireClientSecret = true,

                RedirectUris = { "http://127.0.0.1:5500/signin-oidc" },
                PostLogoutRedirectUris = { "http://127.0.0.1:5500/signout-callback-oidc" },

                AllowedScopes = { "openid", "profile", "email", "api1.read", "api1.write", "offline_access" },
                AllowOfflineAccess = true
            }
        ];
}

