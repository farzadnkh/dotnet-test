using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class MarketCurrencyConfiguration : IEntityTypeConfiguration<MarketCurrency>
{
    public void Configure(EntityTypeBuilder<MarketCurrency> builder)
    {
        builder.ToTable("MarketCurrencies", "dbo");

        builder.HasKey(mc => new { mc.MarketId, mc.CurrencyId });

        builder.HasOne(item => item.Market)
            .WithMany(m => m.MarketCurrencies)
            .HasForeignKey(mc => mc.MarketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(item => item.Currency)
           .WithMany(c => c.MarketCurrencies)
           .HasForeignKey(mc => mc.CurrencyId)
           .OnDelete(DeleteBehavior.NoAction);
    }
}
