using ExchangeRateProvider.Application.Currencies.Handlers.Admin;
using ExchangeRateProvider.Contract.Currencies.Dtos.Responses;
using NT.DDD.Base.Paginations.Responses;
using NT.DDD.Presentation.ApiResponses;

namespace ExchangeRateProvider.Api.Features.Admin;

public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/currencies")
            .WithTags("Currency")
            .WithGroupName(SwaggerConstatns.AdminDefinition)
            .RequireCors("AllowAnyOrigin");

        group.MapGet("/", CurrencyHandlers.GetAllWithPaginationAsync)
            .Produces<APIResponse<IPaginationResponse<List<CurrencyResponse>>>>(StatusCodes.Status200OK)
            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
            .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
            .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
            .WithDescription("Get All Currencies With Pagination.");

        group.MapPost("/", CurrencyHandlers.CreateAsync)
            .Produces<APIResponse<CurrencyResponse>>(StatusCodes.Status200OK)
            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
            .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
            .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
            .WithDescription("Create New Currency");

        group.MapGet("/{id:int}", CurrencyHandlers.GetByIdAsync)
            .Produces<APIResponse<CurrencyResponse>>(StatusCodes.Status200OK)
            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
            .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
            .Produces<APIResponse>(StatusCodes.Status404NotFound)
            .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
            .WithDescription("Get Currency With Id");

        group.MapGet("/{code}", CurrencyHandlers.GetByCodeAsync)
            .Produces<APIResponse<CurrencyResponse>>(StatusCodes.Status200OK)
            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
            .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
            .Produces<APIResponse>(StatusCodes.Status404NotFound)
            .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
            .WithDescription("Get Currency With Code");

        group.MapPut("/{id:int}", CurrencyHandlers.UpdateAsync)
            .Produces<APIResponse<CurrencyResponse>>(StatusCodes.Status200OK)
            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
            .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
            .Produces<APIResponse>(StatusCodes.Status404NotFound)
            .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
            .WithDescription("Update Currency");

        group.MapPatch("deactivate/{id:int}", CurrencyHandlers.DeactivateAsync)
            .Produces<APIResponse<CurrencyResponse>>(StatusCodes.Status200OK)
            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
            .Produces<APIResponse>(StatusCodes.Status401Unauthorized)
            .Produces<APIResponse>(StatusCodes.Status404NotFound)
            .Produces<APIResponse>(StatusCodes.Status500InternalServerError)
            .WithDescription("Deactivate Currency With Id");
    }
}
