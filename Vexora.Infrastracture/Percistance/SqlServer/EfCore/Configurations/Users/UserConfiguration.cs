using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Constants.Users;
using Vexora.Domain.Entities.Users;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Users;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.BaseConfigurationProperties();

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(UserConstants.NameLength);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(UserConstants.NameLength);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(UserConstants.PhoneNumberLength);

        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique();

        builder.Property(x => x.PasswordHasher)
            .IsRequired()
            .HasMaxLength(UserConstants.NameLength);

        builder.Property(x=>x.Role)
            .IsRequired();
    }
}
