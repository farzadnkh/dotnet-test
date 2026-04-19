using System.Net.WebSockets;
using CryptoCompare.Services;
using ExchangeRateProvider.Contract.Contracts.ExchangeRateProviders;
using ExchangeRateProvider.Contract.Contracts.ExchangeRateProviders.Services;
using ExchangeRateProvider.Contract.Contracts.Markets;
using ExchangeRateProvider.Infrastructure.Sql.Background;
using ExchangeRateProvider.Infrastructure.Sql.Repositories.ExchangeRateProviders;
using ExchangeRateProvider.Infrastructure.Sql.Repositories.Markets;
using ExchangeRateProvider.Socket.Commons;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Register services
await builder.ConfigureVaultServerAsync();

builder.Services.AddScoped<IProviderQueryRepository, ProviderQueryRepository>();
builder.Services.AddScoped<IProviderApiAccountQueryRepository, ProviderApiAccountQueryRepository>();
builder.Services.AddScoped<IMarketQueryRepository, MarketQueryRepository>();
builder.Services.AddScoped<IMarketTradingPairQueryRepository, MarketTradingPairQueryRepository>();
builder.Services.AddSingleton<IRateStreamProvider, CryptoCompareSocketRateProvider>();
builder.Services.AddHostedService<RateBackgroundService>();
builder.Services.AddWebSockets(options => { });
var app = builder.Build();

app.Run();