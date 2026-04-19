using ExchangeRateProvider.Domain.Consumers.Entities;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class ConsumerProviderConfiguration : BaseEntityTypeConfiguration<ConsumerProvider>
{
    public override void Configure(EntityTypeBuilder<ConsumerProvider> builder)
    {
        base.Configure(builder);

        builder.Property(cm => cm.IsActive)
            .IsRequired();

        builder.HasOne(cp => cp.Consumer)
            .WithMany(c => c.ConsumerProviders) 
            .HasForeignKey(cp => cp.ConsumerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(item => item.Provider)
            .WithMany()
            .HasForeignKey(cm => cm.ProviderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
