using System.Reflection;
using ExchangeRateProvider.Domain.Consumers.Entities;
using ExchangeRateProvider.Domain.Currencies.Entities;
using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using ExchangeRateProvider.Domain.Markets.Entities;
using ExchangeRateProvider.Domain.Settings.Entities;
using ExchangeRateProvider.Domain.Users.Entities;
using NT.IDP.BaseIdentity.DbContexts;
using NT.IDP.BaseIdentity.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Contexts;

public class ExchangeRateProviderDbContext(
    DbContextOptions<ExchangeRateProviderDbContext> options,
    ILogger<ExchangeRateProviderDbContext> logger) : BaseIdentityDbContext<ExchangeRateProviderDbContext, User, BaseRole<int>, int>(options, logger), IUnitOfWork<int>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }


    #region DbSets

    #region Consumers
    public DbSet<Consumer> Consumers { get; set; }
    public DbSet<ConsumerPair> ConsumerPairs { get; set; }
    public DbSet<ConsumerMarket> ConsumerMarkets { get; set; }
    public DbSet<ConsumerProvider> ConsumersProviders { get; set; }
    #endregion

    #region Currency
    public DbSet<Currency> Currencies { get; set; }
    #endregion

    #region ExchangeRateProviders
    public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }
    public DbSet<Provider>  Providers { get; set; }
    public DbSet<ExchangeRateProviderApiAccount> ExchangeRateProviderApiAccounts { get; set; }
    public DbSet<ProviderBusinessLogic> ProviderBusinessLogics  { get; set; }
    #endregion

    #region Markets
    public DbSet<Market> Markets { get; set; }
    public DbSet<MarketTradingPair> MarketTradingPairs { get; set; }
    public DbSet<MarketCurrency> MarketCurrencies { get; set; }
    public DbSet<MarketProvider> MarketProviders { get; set; }
    public DbSet<MarketTradingPairProvider> MarketTradingPairProviders { get; set; }
    #endregion

    #region Shared
    public DbSet<Setting> Settings { get; set; }
    #endregion

    #endregion
}
