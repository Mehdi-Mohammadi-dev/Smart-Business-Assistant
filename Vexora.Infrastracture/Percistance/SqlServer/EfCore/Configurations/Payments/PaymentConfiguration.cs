using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.Payments;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Payments;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.UserBaseConfigurationProperties();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.Amount)
           .IsRequired();

        builder.Property(x => x.PaymentDate)
           .IsRequired();

        #region Indexes
        builder.HasIndex(x => new
        {
            x.UserId,
            x.PaymentDate
        });
        #endregion

        #region Relations

        builder.HasOne(x=>x.Customer)
            .WithMany()
            .HasForeignKey(x=>x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
