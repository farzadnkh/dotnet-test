using ExchangeRateProvider.Domain.Consumers.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class ConsumerMarketConfiguration : BaseEntityTypeConfiguration<ConsumerMarket>
{
    public override void Configure(EntityTypeBuilder<ConsumerMarket> builder)
    {
        base.Configure(builder);

        builder.Property(cm => cm.IsActive)
            .IsRequired();

        builder.ComplexProperty(cm => cm.SpreadOptions, sd =>
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


        builder.HasOne(cm => cm.Consumer)
            .WithMany(c => c.ConsumerMarkets)
            .HasForeignKey(cm => cm.ConsumerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(cm => cm.Market)
            .WithMany()
            .HasForeignKey(cm => cm.MarketId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
