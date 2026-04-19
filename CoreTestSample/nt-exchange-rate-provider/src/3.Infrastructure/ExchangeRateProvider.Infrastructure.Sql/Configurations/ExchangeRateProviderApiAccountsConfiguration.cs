using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;
using Microsoft.EntityFrameworkCore;
namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class ExchangeRateProviderApiAccountsConfiguration : BaseEntityTypeConfiguration<ExchangeRateProviderApiAccount>
{
    public override void Configure(EntityTypeBuilder<ExchangeRateProviderApiAccount> builder)
    {
        base.Configure(builder);

        builder.Property(erapa => erapa.Owner)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(erapa => erapa.Credentials)
            .HasColumnType("varbinary(max)");

        builder.Property(erapa => erapa.Published)
            .IsRequired();

        builder.Property(erapa => erapa.Description)
            .HasMaxLength(128);

        builder.HasOne(e => e.ExchangeRateProvider)
            .WithMany(e => e.ApiAccounts)
            .HasForeignKey(e => e.ProviderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
