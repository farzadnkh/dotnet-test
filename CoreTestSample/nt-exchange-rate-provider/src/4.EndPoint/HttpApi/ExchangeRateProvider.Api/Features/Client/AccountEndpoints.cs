using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Contract.Users.Dtos.Requests;
using ExchangeRateProvider.Contract.Users.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using NT.DDD.Presentation.ApiResponses;

namespace ExchangeRateProvider.Api.Features.Client
{
    public static class AccountEndpoints
    {
        public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/app/accounts")
                .WithOpenApi()
                .WithTags("Accounts")
                .WithGroupName(SwaggerConstatns.ThirdPartyDefinition)
                .RequireCors("AllowAnyOrigin");

            group.MapPost("login", async ([FromBody] LoginRequest request, [FromServices] IApiKeyClientService apiKeyClientService) =>
            {
                var login = await apiKeyClientService.GenerateApiKeyAsync(request.ClientId, request.ClientSecret, request.Scopes);
                return Results.Ok(new LoginResponse()
                {
                    AccessToken = login.AccessToken,
                    ExpireInSec = login.ExpiresIn
                });
            })
                .Produces<APIResponse<LoginResponse>>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
                .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
                .AllowAnonymous();
        }
    }
}
