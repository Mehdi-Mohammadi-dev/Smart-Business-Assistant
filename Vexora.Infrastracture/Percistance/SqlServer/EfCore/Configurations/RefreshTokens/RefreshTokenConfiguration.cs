using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.RefreshTokens;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.RefreshTokens;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.UserBaseConfigurationProperties();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .IsRequired(false);

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.ExpiresAt });
    }
}
