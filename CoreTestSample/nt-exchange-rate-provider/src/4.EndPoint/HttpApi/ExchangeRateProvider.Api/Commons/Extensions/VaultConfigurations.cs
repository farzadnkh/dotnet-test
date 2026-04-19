using ExchangeRateProvider.Application.Brokers;
using ExchangeRateProvider.Application.Commons;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Settings.Dtos;
using ExchangeRateProvider.Domain.Commons.Events;
using ExchangeRateProvider.Infrastructure.Sql.Commons;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using NT.Caching.Redis.HashiCorpVault.Extensions;
using NT.DDD.Presentation.Commons;
using NT.DDD.Presentation.Swaggers.Bootstrappers;
using NT.HashiCorp.Vault.Abstraction;
using NT.Logs.ElasticApm.HashicorpVault.Extensions;
using NT.Logs.Logging.HashicorpVault.Extensions;
using NT.MassTransit.HashicorpVault.Extensions;
using StackExchange.Redis;

namespace ExchangeRateProvider.Api.Commons.Extensions;

public static class VaultConfigurations
{
    internal static async Task<WebApplicationBuilder> ConfigureVaultServerAsync(this WebApplicationBuilder builder)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        await builder.AddHashiCorpVaultAsync(option =>
        {
            option.EnvironmentFileName = ".env";
            option.JsonFileName = "vault.credentials.json";
            option.VaultServiceTimeout = TimeSpan.FromSeconds(60);
        },
        HachiCorpVaultImplementationsAsync, isDebugMode: environment.Equals("Debug", StringComparison.CurrentCultureIgnoreCase),
            cancellationToken: default);

        return builder;
    }

    private static async Task HachiCorpVaultImplementationsAsync(IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.HealthCheckAsync(cancellationToken: cancellationToken);

        var baseUri = await context.GetBaseUrisAsync(cancellationToken);
        context.Services.AddRazorPages();
        context.Services.AddControllers();
        context.Services.AddMvc()
            .AddRazorRuntimeCompilation();

        await context.AddRedis();
        context.AddPolicy();
        context.AddPresentation();
        context.AddSwagger();
        context.AddAuthentication(baseUri.IdpBaseUri);

        //await context.AddObservabilityAsync(cancellationToken);
        await context.AddInfrastructuresAsync(cancellationToken);
        await context.ConfigureMassTransitAsync(cancellationToken);
        context.AddApplication(cancellationToken);
    }

    private static async Task AddObservabilityAsync(this IHashiCorpVaultContext context, CancellationToken cancellationToken)
    {
        await context.AddLogsAsync(optionAction: options =>
        {
            context.Configurations.GetSection("LoggerOptions").Bind(options);
        },
        pathAction: path =>
        {
            context.Configurations.GetSection("VaultOption:LogsSecrets").Bind(path);
        }, cancellationToken: cancellationToken);

        await context.AddElasticApmServerAsync(optionAction: options =>
        {
            options.HttpDiagnostic.Enable = true;
            options.EfCoreDiagnostic.Enable = true;
            options.ElasticsearchDiagnostic.Enable = true;
            options.SqlClientDiagnostic.Enable = true;
        },
            pathAction: path =>
            {
                context.Configurations.GetSection("VaultOption:ElasticSecrets").Bind(path);
            }
        );
    }

    private static void AddPolicy(this IHashiCorpVaultContext context)
    {
        context.Services.AddCors(options => options.AddPolicy("AllowAllOrigins", builder =>
          builder.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader()));
    }


    private static void AddPresentation(this IHashiCorpVaultContext context)
    {
        context.Services.AddPresentation(options =>
        {
            options.ShowExceptionDetail = !context.Builder.Environment.IsProduction();
            options.IgnoreResponseRoutes = [];
            options.IgnoreRequestRoutes = [];
        });
    }

    private static void AddSwagger(this IHashiCorpVaultContext context)
    {
        context.Services.AddEndpointsApiExplorer();
        context.Services.AddSwaggerGen();
        context.Services.AddSwaggerWithFilterAndOAuth(opt =>
        {
            opt.SwaggerGenOptions = options =>
            {
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                options.SwaggerDoc($"{SwaggerConstatns.AdminDefinition}", new OpenApiInfo { Title = $"{SwaggerConstatns.AdminDefinition}", Version = "v1" });
                options.SwaggerDoc($"{SwaggerConstatns.ThirdPartyDefinition}", new OpenApiInfo { Title = $"{SwaggerConstatns.ThirdPartyDefinition}", Version = "v1" });
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var groupName = apiDesc.GroupName;
                    return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
                });
                options.EnableAnnotations();
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            };

            opt.DefaultApiResponseOptions = new DefaultApiResponseOptions
            {
                SetDefault400 = true,
                SetDefault401 = true,
                SetDefault403 = true,
                SetDefault404 = true
            };

            opt.GeneralSwaggerOptions = new GeneralSwaggerOptions
            {
                DefaultVersion = "V1"
            };
        });
    }

    private static void AddAuthentication(this IHashiCorpVaultContext context, string idpbaseUrl)
    {
        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = idpbaseUrl;
                    config.TokenValidationParameters = new()
                    {
                        ValidateAudience = false,
                        ValidateIssuer = true,
                        ValidateLifetime = true, 
                        ValidateIssuerSigningKey = true,
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };

                    config.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrEmpty(accessToken) &&
                                (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["Accept"] == "text/event-stream"))
                            {
                                context.Token = context.Request.Query["access_token"];
                            }

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var claims = context.Principal.Claims;
                            var scopeClaim = claims.FirstOrDefault(c => c.Type == "scope")?.Value;

                            if (string.IsNullOrEmpty(scopeClaim))
                            {
                                context.Fail("Scope claim is missing from the token.");
                                return Task.CompletedTask;
                            }

                            var requiredScopes = new List<string> { "realtime-api" };

                            var tokenScopes = scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                            if (!requiredScopes.All(rs => tokenScopes.Contains(rs)))
                            {
                                context.Fail("Required scopes are missing from the token.");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

        context.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        });
    }

    private static async Task<IHashiCorpVaultContext> ConfigureMassTransitAsync(
        this IHashiCorpVaultContext context,
        CancellationToken cancellationToken)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Debug";
        await context.AddMassTransitAsync(rmo =>
        {
            rmo.ServiceName = $"ExchangeRateProvider_{env}_api";
            rmo.ServiceReleaseEnvironment = env;
        }, busRegConf =>
        {
            busRegConf.AddConsumer<PriceChangedEventConsumer>();
            busRegConf.AddConsumer<SocketSyncEventConsumer>();
            busRegConf.AddConsumer<NewPriceChangeStreamedEventConsumer>();
            busRegConf.AddConsumer<SyncSettingsEventConsumer>();
        }, (busRegContext, rabbitBusFactoryConf) =>
        {
            rabbitBusFactoryConf.Message<PriceChangedEventMessageArgs>(x => x.SetEntityName($"{env}_PriceChangeQueue"));
            rabbitBusFactoryConf.Message<NewPriceChangeStreamedMessageArgs>(x => x.SetEntityName($"{env}_NewPriceChangeStreamQueue"));
            rabbitBusFactoryConf.Message<SocketSyncMessageArgs>(x => x.SetEntityName($"{env}_SocketSyncQueue"));
            rabbitBusFactoryConf.Message<SettingModel>(x => x.SetEntityName($"{env}_SettingSyncQueue"));

            rabbitBusFactoryConf.ReceiveEndpoint($"{env}_PriceChangeQueue", opt =>
            {
                opt.ConfigureConsumer<PriceChangedEventConsumer>(busRegContext);
            });

            rabbitBusFactoryConf.ReceiveEndpoint($"{env}_SocketSyncQueue", opt =>
            {
                opt.ConfigureConsumer<SocketSyncEventConsumer>(busRegContext);
            });
            rabbitBusFactoryConf.ReceiveEndpoint($"{env}_NewPriceChangeStreamQueue", opt =>
            {
                opt.ConfigureConsumer<NewPriceChangeStreamedEventConsumer>(busRegContext);
            });
            rabbitBusFactoryConf.ReceiveEndpoint($"{env}_SettingSyncQueue", opt =>
            {
                opt.ConfigureConsumer<SyncSettingsEventConsumer>(busRegContext);
            });
        }, pathAction =>
        {
            context.Configurations.GetSection("VaultOption:MasstransitSecrets").Bind(pathAction);
        });

        return context;
    }
}
