using ExchangeRateProvider.Domain.Consumers.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateProvider.Infrastructure.Sql.Configurations;

public class ConsumerConfiguration : BaseEntityTypeConfiguration<Consumer>
{
    public override void Configure(EntityTypeBuilder<Consumer> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.ProjectName)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.Apikey).HasMaxLength(int.MaxValue);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.ConsumerMarkets)
            .WithOne(cm => cm.Consumer)
            .HasForeignKey(cm => cm.ConsumerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.ConsumerProviders)
            .WithOne(cp => cp.Consumer)
            .HasForeignKey(cp => cp.ConsumerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.ConsumerPairs)
            .WithOne(cp => cp.Consumer)
            .HasForeignKey(cp => cp.ConsumerId)
            .OnDelete(DeleteBehavior.NoAction);;
    }
}
