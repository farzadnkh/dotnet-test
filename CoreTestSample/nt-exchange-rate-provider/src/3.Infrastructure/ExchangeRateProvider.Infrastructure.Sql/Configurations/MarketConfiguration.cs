using ExchangeRateProvider.Domain.Markets.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class MarketConfiguration : BaseEntityTypeConfiguration<Market>
{
    public override void Configure(EntityTypeBuilder<Market> builder)
    {
        base.Configure(builder);
        builder.Property(m => m.CalculationTerm)
            .IsRequired();

        builder.Property(m => m.IsDefault)
            .IsRequired();

        builder.Property(m => m.Published)
            .IsRequired();

        builder.Property(m => m.RatingMethod)
            .IsRequired();

        builder.ComplexProperty(m => m.SpreadOptions, sd =>
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


        builder.HasOne(item => item.BaseCurrency)
            .WithMany()
            .HasForeignKey(m => m.BaseCurrencyId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
