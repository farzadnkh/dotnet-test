using NT.HashiCorp.Vault.Abstraction;
using NT.KYC.Jibit.HashiCorpVault.Extensions;
using NT.SDK.RestClient.Logger;
using sample.NT.KYC.Jibit.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Action<ApiLogger> logger = apiLogger =>
{
    // Implement your logging logic here
    // For example, write to console or file
    Console.WriteLine($"Log Level: {apiLogger.LogLevel}, Message: {apiLogger.Message}");
};

builder.Services.AddMemoryCache();
builder.Services.AddTransient<IJibitService, JibitService>();
builder.Services.AddTransient<JibitCredentialsManagement>();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
await builder.AddHashiCorpVaultAsync(
    options =>
    {
        options.EnvironmentFileName = ".env";
        options.JsonFileName = "vault.credentials.json";
        options.VaultServiceTimeout = TimeSpan.FromSeconds(60);
    },
    async (context, cancellationToken) =>
    {
        await HashiCorpVaultImplementationsAsync(context, cancellationToken);
    },true);

var app = builder.Build();

app.MapControllers();
// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.Run();

async Task HashiCorpVaultImplementationsAsync(
    IHashiCorpVaultContext context,
    CancellationToken cancellationToken = default
)
{
    await context.HealthCheckAsync(cancellationToken);

    await context.AddJibitClientAsync(builder.Environment, opt =>
    {
        opt.JibitRestClientLifeCycle = ServiceLifetime.Scoped;
        opt.JibitAlphaRestClientLifeCycle = ServiceLifetime.Scoped;
        opt.Logger = logger;
        opt.JibitBasePath = "https://tpp.nt-test.dev/jibit/ide";
        opt.Timeout = 60000;
        opt.JibitAlphaBasePath = "https://tpp.nt-test.dev/Jibit-alpha/alpha/api";        
    },
    pathAction: path =>
    {
        path.ConnectionKey = "JibitCredentials";
    });
}