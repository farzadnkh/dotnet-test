using Duende.IdentityModel.Client;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using ExchangeRateProvider.Contract.Commons.Helpers;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Consumers.Dtos;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Security.Cryptography;

namespace ExchangeRateProvider.Application.Consumers.Services;

public class ApiKeyClientService(
    ConfigurationDbContext configurationDb,
    BaseUri baseUri,
    IUserCommandRepository userCommand,
    ILogger<ApiKeyClientService> logger) : IApiKeyClientService
{
    public async Task<TokenResponse> GenerateApiKeyAsync(string clientId, string clientSecret, string scopes)
    {
        var identityServerUrl = $"{baseUri.IdpBaseUri.Trim('/')}";

        using var client = new HttpClient();

        await userCommand.RemoveUserPersistedGrantAsync(clientId, default);

        var disco = await client.GetDiscoveryDocumentAsync(identityServerUrl);
        if (disco.IsError)
        {
            logger.LogCritical($"Discovery error: {disco.Error}");
            throw ApplicationBadRequestException.Create("an error occurred while trying to login. Please check Logs.");
        }

        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = scopes
        });

        if (tokenResponse.IsError)
        {
            logger.LogError($"Token error: {tokenResponse.Error}");
            throw ApplicationBadRequestException.Create($"{tokenResponse.Error}");
        }

        return tokenResponse;
    }

    public async Task<Credentials> GenerateClientSecretAsync(string username, string projectName, int userId, int consumerId, string encryptionKey, CancellationToken cancellationToken)
    {
        var hashedClientId = ClientIdHelper.CreateAndEncryptClinetId(username, projectName, userId, consumerId, encryptionKey);
        string rawSecret = GenerateSecureSecret();
        string hashedSecret = rawSecret.Sha256();

        var existClient = configurationDb.Clients.Include(c => c.ClientSecrets).SingleOrDefault(c => c.ClientId == hashedClientId);
        if (existClient != null)
        {
            await userCommand.RemoveUserPersistedGrantAsync(hashedClientId, default);

            existClient.ClientSecrets = [new() {
                ClientId = existClient.Id,
                Created = DateTime.UtcNow,
                Description = null,
                Expiration = null,
                Type = "SharedSecret",
                Value = hashedSecret,
            }];
            configurationDb.Update(existClient);
        }
        else
        {
            Client client = new()
            {
                ClientId = hashedClientId,
                ClientName = $"{username}_Client",
                ClientSecrets = { new Secret(hashedSecret) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequirePkce = true,
                RequireClientSecret = true,

                RedirectUris = { $"{baseUri.IdpBaseUri.Trim('/')}/signin-oidc" },
                PostLogoutRedirectUris = { $"{baseUri.IdpBaseUri.Trim('/')}/signout-callback-oidc" },

                AllowedScopes = { "openid", "profile", "email", "realtime-api" },
                AllowOfflineAccess = true,
                AccessTokenType = AccessTokenType.Reference,
                Enabled = true,
                AccessTokenLifetime = 86400
            };

            configurationDb.Clients.Add(client.ToEntity());
        }
        
        await configurationDb.SaveChangesAsync(cancellationToken);
        return new()
        {
            ClientId = hashedClientId,
            ClientSecret = rawSecret
        };
    }

    private string GenerateSecureSecret(int length = 64)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
