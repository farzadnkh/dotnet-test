using ExchangeRateProvider.Api.Features.Admin;
using ExchangeRateProvider.Api.Features.Client;
using ExchangeRateProvider.Application.Sockets.Clients;
using ExchangeRateProvider.Infrastructure.Sql.Commons;
using NT.DDD.Presentation.ApiResponses;
using System.Net.WebSockets;

namespace ExchangeRateProvider.Api.Commons.Extensions;

public static class HostConfigurations
{
    public static void UseApp(this WebApplication app)
    {
        app.UseStaticFiles();
        app.UseRouting();

        app.MapRazorPages()
         .WithStaticAssets()
         .RequireAuthorization();

        app.RegisterEndPoints();

        app.UseInfrastructure();
        app.UseAuthentication();
        app.UseCors("AllowOrigin");
        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{SwaggerConstatns.AdminDefinition}/swagger.json", SwaggerConstatns.AdminDefinition);
                c.SwaggerEndpoint($"/swagger/{SwaggerConstatns.ThirdPartyDefinition}/swagger.json", SwaggerConstatns.ThirdPartyDefinition);
            });
        }
        app.UseAuthorization();

        app.UseWhen(
           context => (context.IsSpesificPath()),
           branch =>
           {
               branch.UseApiResponseMiddleware();
           });

        app.MapControllerRoute(
         name: "default",
         pattern: "{controller=Home}/{action=Index}/{id?}");

        app.UseHttpsRedirection();

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30)
        };
        app.UseWebSockets(webSocketOptions);

        app.Map("/wss/price", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("WebSocket connection expected.");
                return;
            }

            var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();

            try
            {
                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                await handler.HandleAsync(context, socket);
            }
            catch (WebSocketException wex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(wex, "WebSocket error occurred while handling connection.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Unexpected error in WebSocket handler.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        });
    }

    private static void RegisterEndPoints(this WebApplication app)
    {
        app.MapCurrencyEndpoints();
        app.MapTradeEndpoints();
        app.MapAccountEndpoints();
    }

    private static bool IsSpesificPath(this HttpContext context)
    {
        List<string> spesificPath = ["api/v1"];

        foreach (var item in spesificPath)
            if (context.Request.Path.Value.StartsWith(item))
                return true;
        return false;
    }
}
