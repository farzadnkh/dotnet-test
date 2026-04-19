using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class MarketTradingPairProviderConfiguration : IEntityTypeConfiguration<MarketTradingPairProvider>
{
    public void Configure(EntityTypeBuilder<MarketTradingPairProvider> builder)
    {
        builder.ToTable("MarketTradingPairProviders", "dbo");

        builder.HasKey(mc => new { mc.MarektTradingPairId, mc.ExchangeRateProviderId });

        builder
            .HasOne(x => x.MarketTradingPair)
            .WithMany(x => x.MarketTradingPairProviders)
            .HasForeignKey(x => x.MarektTradingPairId)
            .OnDelete(DeleteBehavior.NoAction);


        builder
            .HasOne(x => x.ExchangeRateProvider)
            .WithMany(x => x.MarketTradingPairProviders)
            .HasForeignKey(x => x.ExchangeRateProviderId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
