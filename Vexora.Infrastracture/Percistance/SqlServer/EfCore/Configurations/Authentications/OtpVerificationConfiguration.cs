using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vexora.Domain.Entities.Authentications;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Bases;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Authentications;

public sealed class OtpVerificationConfiguration
    : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.BaseConfigurationProperties();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Purpose)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.UsedAt)
            .IsRequired(false);

        builder.Property(x => x.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.PhoneNumber,
            x.Purpose,
            x.ExpiresAt
        });
    }
}
