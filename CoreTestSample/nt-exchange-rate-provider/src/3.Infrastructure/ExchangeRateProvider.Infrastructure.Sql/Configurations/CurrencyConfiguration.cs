using ExchangeRateProvider.Domain.Currencies.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class CurrencyConfiguration : BaseEntityTypeConfiguration<Currency>
{
    public override void Configure(EntityTypeBuilder<Currency> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(c => c.Symbol)
            .HasMaxLength(8);

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.Published)
            .IsRequired();

        builder.HasIndex(t => t.Code).IsUnique();

    }
}
