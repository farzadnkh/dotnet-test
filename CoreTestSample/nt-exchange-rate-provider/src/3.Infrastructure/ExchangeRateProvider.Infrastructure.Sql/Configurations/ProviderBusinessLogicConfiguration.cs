using ExchangeRateProvider.Domain.ExchangeRateProviders.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class ProviderBusinessLogicConfiguration : BaseEntityTypeConfiguration<ProviderBusinessLogic>
{
    public override void Configure(EntityTypeBuilder<ProviderBusinessLogic> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
          .IsRequired()
          .HasMaxLength(64);

        builder.HasOne(e => e.ExchangeRateProvider)
            .WithMany(e => e.ProviderBusinessLogics)
            .HasForeignKey(e => e.ProviderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}