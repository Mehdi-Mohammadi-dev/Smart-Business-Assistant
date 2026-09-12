using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Constants.Sales.Items;
using Vexora.Domain.Entities.Sales.Items;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Sales.Items;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.BaseConfigurationProperties();

        builder.Property(x => x.Description)
            .IsRequired(false)
            .HasMaxLength(SaleItemConstants.DescriptionLength);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .IsRequired();

        builder.Property(x => x.SaleId)
            .IsRequired();
    }
}
