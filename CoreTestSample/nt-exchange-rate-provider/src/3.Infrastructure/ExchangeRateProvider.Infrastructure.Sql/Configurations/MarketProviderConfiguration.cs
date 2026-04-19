using System.Reflection.Emit;
using ExchangeRateProvider.Domain.Markets.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class MarketProviderConfiguration : IEntityTypeConfiguration<MarketProvider>
{
    public void Configure(EntityTypeBuilder<MarketProvider> builder)
    {
        builder.ToTable("MarketProviders", "dbo");

        builder.HasKey(mc => new { mc.MarketId, mc.ExchangeRateProviderId });
        
        builder
            .HasOne(x => x.Market)
            .WithMany(x => x.MarketExchangeRateProviders)
            .HasForeignKey(x => x.MarketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(x => x.ExchangeRateProvider)
            .WithMany(x => x.MarketExchangeRateProviders)
            .HasForeignKey(x => x.ExchangeRateProviderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
