using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class MarketTradingPairsConfiguration : BaseEntityTypeConfiguration<Provider>
{
    public override void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("ExchangeRateProviders", "dbo");

        builder.Property(erp => erp.Name)
                    .IsRequired()
                    .HasMaxLength(64);

        builder.Property(erp => erp.Type)
            .IsRequired();

        builder.Property(erp => erp.Published)
            .IsRequired();
    }
}
