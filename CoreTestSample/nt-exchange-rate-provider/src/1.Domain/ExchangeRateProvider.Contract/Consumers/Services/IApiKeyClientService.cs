using Duende.IdentityModel.Client;
using ExchangeRateProvider.Contract.Consumers.Dtos;

namespace ExchangeRateProvider.Contract.Consumers.Services
{
    public interface IApiKeyClientService
    {
        Task<Credentials> GenerateClientSecretAsync(string username, string projectName, int userId, int consumerId, string encryptionKey, CancellationToken cancellationToken);
        Task<TokenResponse> GenerateApiKeyAsync(string clientId, string clientSecret, string scopes);
    }
}
