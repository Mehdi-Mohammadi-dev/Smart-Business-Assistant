using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.SubscriptionPlans;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.SubscriptionPlans;

public sealed class ReferralSettingConfiguration : IEntityTypeConfiguration<ReferralSetting>
{
    public void Configure(EntityTypeBuilder<ReferralSetting> builder)
    {
        builder.BaseConfigurationProperties();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.RewardDays).IsRequired();
        builder.Property(x => x.MaxRewardDaysPerUser).IsRequired();
    }
}
