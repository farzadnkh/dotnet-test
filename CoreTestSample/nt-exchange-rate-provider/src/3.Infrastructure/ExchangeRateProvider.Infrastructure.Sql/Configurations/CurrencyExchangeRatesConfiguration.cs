using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class CurrencyExchangeRatesConfiguration : BaseEntityTypeConfiguration<CurrencyExchangeRate>
{
    public override void Configure(EntityTypeBuilder<CurrencyExchangeRate> builder)
    {
        base.Configure(builder);

        builder.Property(cer => cer.ConsumerId)
                    .IsRequired();

        builder.Property(cer => cer.MarketTradingPairId)
            .IsRequired();
        
        builder.Property(cer => cer.OriginalRate)
            .HasColumnType("decimal(18, 2)");

        builder.Property(cer => cer.Buy)
            .HasColumnType("decimal(18, 2)");

        builder.Property(cer => cer.BuyRateChange)
            .IsRequired();

        builder.Property(cer => cer.Sell)
            .HasColumnType("decimal(18, 2)");
        
        
        builder.HasIndex(cer => new { cer.ConsumerId, cer.MarketTradingPairId })
            .IsUnique(false)
            .IsClustered(false);
    }
}
