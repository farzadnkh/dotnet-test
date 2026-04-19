using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class MarketTradingPairConfiguration : BaseEntityTypeConfiguration<MarketTradingPair>
{
    public override void Configure(EntityTypeBuilder<MarketTradingPair> builder)
    {
        base.Configure(builder);

        builder.Property(mtp => mtp.Published)
            .IsRequired();

        builder.ComplexProperty(mtp => mtp.SpreadOptions, sd =>
        {
            sd.IsRequired();
            sd.Property(x => x.UpperLimitPercentage)
                .HasColumnName("UpperLimitPercentage")
                .HasColumnType("decimal(18, 2)");

            sd.Property(x => x.SpreadEnabled)
                .HasDefaultValue(false)
                .HasColumnName("SpreadEnabled");

            sd.Property(x => x.LowerLimitPercentage)
                .HasColumnName("LowerLimitPercentage")
                .HasColumnType("decimal(18, 2)");
        });

        builder.Property(mtp => mtp.LastModifierUserId);

        builder.Property(mtp => mtp.UpdatedOnUtc);

        builder.HasOne(item => item.Market)
            .WithMany(m => m.TradingPairs)
            .HasForeignKey(mtp => mtp.MarketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(item => item.Currency)
            .WithMany(c => c.MarketTradingPairs)
            .HasForeignKey(mtp => mtp.CurrencyId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
