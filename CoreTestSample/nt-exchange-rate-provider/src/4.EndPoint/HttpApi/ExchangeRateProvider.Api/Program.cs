using ExchangeRateProvider.Api.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);
await builder.ConfigureVaultServerAsync();

var app = builder.Build();
app.UseApp();
await app.RunAsync();