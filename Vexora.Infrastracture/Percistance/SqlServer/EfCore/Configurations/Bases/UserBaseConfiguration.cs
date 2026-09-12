using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.Users;
using Vexora.Domain.sharedKernel.BaseEntities;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

public static class UserBaseConfiguration
{
    public static void UserBaseConfigurationProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
      where TEntity : UserBaseEntity
    {
        builder.BaseConfigurationProperties();

        builder.Property(x => x.UserId) 
            .IsRequired();

        #region Relations

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
