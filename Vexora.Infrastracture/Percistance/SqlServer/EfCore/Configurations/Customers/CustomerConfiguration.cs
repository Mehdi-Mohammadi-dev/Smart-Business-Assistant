using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Constants.Customers;
using Vexora.Domain.Entities.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Customers;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.UserBaseConfigurationProperties();

        builder.Property(x => x.FirstName)
            .HasMaxLength(CustomerConstants.NameLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(CustomerConstants.NameLength)
            .IsRequired();
    }
}
