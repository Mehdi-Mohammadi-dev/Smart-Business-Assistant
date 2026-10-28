using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.SubscriptionPlans;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.SubscriptionPlans;

public sealed class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.BaseConfigurationProperties();
        builder.Property(x => x.ReferrerUserId).IsRequired();
        builder.Property(x => x.ReferredUserId).IsRequired();
        builder.Property(x => x.RewardGranted).IsRequired();
        builder.Property(x => x.RewardDays).IsRequired();
        builder.HasIndex(x => x.ReferredUserId).IsUnique();
        builder.HasIndex(x => x.ReferrerUserId);
        builder.HasOne<Vexora.Domain.Entities.Users.User>().WithMany().HasForeignKey(x => x.ReferrerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Vexora.Domain.Entities.Users.User>().WithMany().HasForeignKey(x => x.ReferredUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
