using ExchangeRateProvider.EndPoint.ApiGateway.App.Commons;

var builder = WebApplication.CreateBuilder(args);

await builder.ConfigureVaultServerAsync();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseWebSockets();
app.MapReverseProxy();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/ExchangeRateProvider.ThirdParty/swagger.json", "ExchangeRateProvider.ThirdParty");
});

app.UseHttpsRedirection();
app.MapControllers();
await app.RunAsync();
