using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateProvider.Api.Seeders
{
    public static class IdpSeedData
    {
        public static void EnsureSeedData(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var services = serviceScope.ServiceProvider;

            void SeedDb<TContext>(Action<TContext> seeder) where TContext : DbContext
            {
                var context = services.GetRequiredService<TContext>();
                context.Database.Migrate();
                seeder(context);
            }

            SeedDb<ConfigurationDbContext>(context =>
            {
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                        context.Clients.Add(client.ToEntity());
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                        context.IdentityResources.Add(resource.ToEntity());
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var scope in Config.GetApiScopes())
                        context.ApiScopes.Add(scope.ToEntity());
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                        context.ApiResources.Add(resource.ToEntity());
                    context.SaveChanges();
                }
            });

            SeedDb<PersistedGrantDbContext>(_ => { });
        }
    }
}
