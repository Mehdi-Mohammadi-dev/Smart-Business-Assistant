using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.sharedKernel.BaseEntities;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

public static class BaseEntityConfiguration
{
    public static void BaseConfigurationProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
      where TEntity : BaseEntity
    {
        builder.Property(x => x.Id)
            .IsRequired();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreateAt)
            .IsRequired();

        builder.Property(x => x.UpdateAt)
            .IsRequired();

        builder.Property(x => x.IsDelete)
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .IsRequired(false);
    }
}
