using ExchangeRateProvider.Domain.Consumers.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class ConsumersPairsConfiguration : BaseEntityTypeConfiguration<ConsumerPair>
{
    public override void Configure(EntityTypeBuilder<ConsumerPair> builder)
    {
        base.Configure(builder);

        builder.Property(cp => cp.IsActive)
            .IsRequired();

        builder.ComplexProperty(cp => cp.SpreadOptions, sd =>
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

        builder.HasOne(cp => cp.Consumer)
            .WithMany(c => c.ConsumerPairs)
            .HasForeignKey(cp => cp.ConsumerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(item => item.Market)
            .WithMany()
            .HasForeignKey(cp => cp.MarketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(item => item.MarketTradingPair)
            .WithMany()
            .HasForeignKey(cp => cp.PairId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
