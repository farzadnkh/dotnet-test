using Duende.IdentityServer.EntityFramework.DbContexts;
using ExchangeRateProvider.Application.Brokers;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Domain.Users.Entities;
using ExchangeRateProvider.Infrastructure.Sql.Contexts;
using ExchangeRateProvider.Infrastructure.Sql.Repositories.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NT.Caching.Redis.HashiCorpVault.Extensions;
using NT.HashiCorp.Vault.Abstraction;
using NT.IDP.BaseIdentity.Bootstrapper;
using NT.IDP.BaseIdentity.Entities;
using NT.IDP.BaseIdentity.HashiCorp.Vault.Extensions;
using NT.IDP.BaseIdentityServer.HashiCorp.Vault.Extensions;
using NT.MassTransit.HashicorpVault.Extensions;
using System.IO;

namespace ExchangeRateProvider.Infrastructure.Sql.Commons;

public static class InjectionBootstrapper
{
    public static async Task<IHashiCorpVaultContext> AddInfrastructuresAsync(
        this IHashiCorpVaultContext context,
        CancellationToken cancellationToken = default)
    {
        context.Services.Scan(scan => scan
            .FromAssembliesOf(typeof(ConsumerQueryRepository))
            .AddClasses()
            .AsSelf()
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        await context.AddBaseInfraAsync(cancellationToken);
        await context.AddIdpAsync(cancellationToken);

        return context;
    }
    public static void UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseIdentityServer();
        app.UseAuthorization();
    }

    public static async Task<BaseUri> GetBaseUrisAsync(this IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        BaseUri result = new();
        var globalMountPoint = context.Configurations["VaultOption:GlobalCredentialsMountPoint"];

        await context.GetCredentialsAsync<BaseUri>(nameof(BaseUri), t =>
        {
            context.Services.AddSingleton(t);
            result = t;
        }, credentialsMountPoint: globalMountPoint, cancellationToken: cancellationToken);

        return result;
    }

    public static async Task AddRedis(this IHashiCorpVaultContext context)
    {
        var redisOptions = context.Configurations.GetSection("RedisOption").Get<RedisOption>() ?? new();
        await context.AddStackExchangeRedisAsync(optionAction: opt =>
        {
            opt.Database = redisOptions.DefaultDatabase;

        }, pathAction: path =>
        {
            context.Configurations.GetSection("VaultOption:RedisSecrets").Bind(path);
        });
    }
    #region Utilities

    private static async Task AddIdpAsync(this IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        var migrationAssembley = typeof(ExchangeRateProviderDbContext).Assembly.GetName().Name!;
        await context.AddIdentitySqlDbContextAsync<ExchangeRateProviderDbContext, int>(
            optionAction: option =>
            {
                option.SqlServerOptionsAction = p => p.MigrationsAssembly(migrationAssembley);
            },
            pathAction: path =>
            {
                context.Configurations.GetSection("VaultOption:MsSqlSecrets").Bind(path);
            },
            cancellationToken: cancellationToken);

        context.Services.AddBaseIdentity<ExchangeRateProviderDbContext, User, BaseRole<int>, int>()
        .AddEntityFrameworkStores<ExchangeRateProviderDbContext>()
        .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);

        var idsBuilder = await context.AddIdsSqlDbContextsAsync<ConfigurationDbContext, PersistedGrantDbContext>(
            optionAction: option =>
            {
                option.MigrationAssemblyFullName = migrationAssembley;
            },
            pathAction: path =>
            {
                context.Configurations.GetSection("VaultOption:MsSqlSecrets").Bind(path);
            },
            cancellationToken: cancellationToken);

        idsBuilder.AddAspNetIdentity<User>()
            .AddDeveloperSigningCredential();

        idsBuilder.Services.AddAuthorization();
        idsBuilder.Services.AddAuthentication();
    }

    private static async Task AddBaseInfraAsync(this IHashiCorpVaultContext context, CancellationToken cancellationToken = default)
    {
        var globalMountPoint = context.Configurations["VaultOption:GlobalCredentialsMountPoint"];
        await context.GetCredentialsAsync<EncryptionConfiguration>(nameof(EncryptionConfiguration), t =>
        {
            context.Services.AddSingleton(t);
        }, credentialsMountPoint: globalMountPoint, cancellationToken: cancellationToken);

        context.Services.AddScoped<IUnitOfWork<int>, ExchangeRateProviderDbContext>();
    }
    #endregion
}
