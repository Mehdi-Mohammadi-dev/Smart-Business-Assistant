using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.Sales;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Sales;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.UserBaseConfigurationProperties();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.SaleDate)
            .IsRequired();

        #region Relations

        builder.HasMany(x=>x.Items)
            .WithOne()
            .HasForeignKey(x=>x.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

    }
}
