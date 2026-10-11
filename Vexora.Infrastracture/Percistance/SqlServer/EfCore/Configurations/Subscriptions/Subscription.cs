using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Subscriptions;

public sealed class SubscriptionConfiguration
    : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.UserBaseConfigurationProperties();

        builder.Property(x => x.PlanType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StartAt)
            .IsRequired();

        builder.Property(x => x.EndAt)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Status });
    }
}
