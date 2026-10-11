using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Authentications;
using Vexora.Domain.Entities.Authentications;
using Vexora.Domain.Enums.Authentications;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Authentications;

public sealed class OtpVerificationRepository(
    ApplicationDbContext context) : IOtpVerificationRepository
{
    public async Task<OtpVerification?> GetLatestUnusedAsync(
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        return await context.OtpVerifications
            .Where(x =>
                x.PhoneNumber == phoneNumber &&
                x.Purpose == purpose &&
                x.UsedAt == null)
            .OrderByDescending(x => x.CreateAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        OtpVerification otp,
        CancellationToken cancellationToken = default)
    {
        await context.OtpVerifications.AddAsync(otp, cancellationToken);
    }
}
