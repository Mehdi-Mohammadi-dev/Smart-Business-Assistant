using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Constants.Checks;
using Vexora.Domain.Entities.Checks;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Checks;

public sealed class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder.UserBaseConfigurationProperties();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.DueDate)
            .IsRequired();

        builder.Property(x => x.CheckNumber)
            .IsRequired(false)
            .HasMaxLength(CheckConstants.CheckNumberLength);

        #region Indexes

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CustomerId
        });

        #endregion

        #region Relations

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
