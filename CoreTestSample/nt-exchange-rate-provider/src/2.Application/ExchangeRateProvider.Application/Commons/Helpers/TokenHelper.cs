using ExchangeRateProvider.Contract.Commons.Helpers;
using Microsoft.AspNetCore.Http;
using NT.DDD.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;

namespace ExchangeRateProvider.Application.Commons.Helpers
{
    public static class TokenHelper
    {
        public static string GetToken(this HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();

            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrWhiteSpace(accessToken) &&
                (context.WebSockets.IsWebSocketRequest || context.Request.Headers.Accept == "text/event-stream"))
            {
                token = context.Request.Query["access_token"];
            }

            return token;
        }

        public static int GetUserId(this HttpContext context, string encryptionKey, string token = null)
        {
            if (string.IsNullOrWhiteSpace(token))
                token = context.GetToken();

            try
            {
                string[] clientIdClaims = GetClientIdFromToken(token, encryptionKey);

                if (clientIdClaims is not null && int.TryParse(clientIdClaims[1], out int userId))
                    return userId;
                else
                    throw DomainBadRequestException.Create("UserId not Found.");
            }
            catch (Exception ex)
            {
                throw DomainBadRequestException.Create("Failed To retrieve User Id", ex);
            }
        }

        public static int GetUserId(string clientId, string encryptionKey)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw ApplicationBadRequestException.Create("An error occurred while trying to connect.");

            try
            {
                var clientIdClaims = ClientIdHelper.GetClientId(clientId, encryptionKey).Split('_');

                if (clientIdClaims is not null && int.TryParse(clientIdClaims[1], out int userId))
                    return userId;
                else
                    throw DomainBadRequestException.Create("UserId not Found.");
            }
            catch (Exception ex)
            {
                throw DomainBadRequestException.Create("Failed To retrieve User Id", ex);
            }
        }

        public static int GetConsumerId(this HttpContext context, string encryptionKey, string token = null)
        {
            if (string.IsNullOrWhiteSpace(token))
                token = context.GetToken();

            try
            {
                var clientIdClaims = GetClientIdFromToken(token, encryptionKey);

                if (clientIdClaims is not null && int.TryParse(clientIdClaims.Last(), out int consumerId))
                    return consumerId;
                else
                    throw DomainBadRequestException.Create("UserId not Found.");
            }
            catch (Exception ex)
            {
                throw DomainBadRequestException.Create("Failed To retrieve User Id", ex);
            }
        }

        public static int GetConsumerId(string clientId, string encryptionKey)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw ApplicationBadRequestException.Create("An error occurred while trying to connect.");

            try
            {
                var clientIdClaims = ClientIdHelper.GetClientId(clientId, encryptionKey).Split('_');

                if (clientIdClaims is not null && int.TryParse(clientIdClaims.Last(), out int consumerId))
                    return consumerId;
                else
                    throw DomainBadRequestException.Create("UserId not Found.");
            }
            catch (Exception ex)
            {
                throw DomainBadRequestException.Create("Failed To retrieve User Id", ex);
            }
        }

        private static string[] GetClientIdFromToken(string token, string encryptionKey)
        {
            var handler = new JwtSecurityTokenHandler();
            var readToken = handler.ReadJwtToken(token);

            var userClientId = readToken.Claims.FirstOrDefault(x => x.Type == "client_id").Value;
            var clientIdClaims = ClientIdHelper.GetClientId(userClientId, encryptionKey).Split('_');
            return clientIdClaims;
        }
    }
}
