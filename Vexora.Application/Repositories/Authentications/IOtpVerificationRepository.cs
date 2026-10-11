using Vexora.Domain.Entities.Authentications;
using Vexora.Domain.Enums.Authentications;

namespace Vexora.Application.Repositories.Authentications;

public interface IOtpVerificationRepository
{
    Task<OtpVerification?> GetLatestUnusedAsync(
    string phoneNumber,
    OtpPurpose purpose,
    CancellationToken cancellationToken = default);

    Task AddAsync(
        OtpVerification otp,
        CancellationToken cancellationToken = default);
}
