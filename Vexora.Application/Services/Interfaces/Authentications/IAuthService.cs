using Vexora.Application.Dtos.Authentications;
using Vexora.Application.Dtos.Authentications.Otps;
using Vexora.Application.Dtos.Authentications.Profiles;

namespace Vexora.Application.Services.Interfaces.Authentications;

public interface IAuthService
{
    Task<SendOtpResultDto> SendOtpAsync(SendOtpDto dto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> RefreshAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);

    Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(RefreshTokenDto dto,string accessTokenId,DateTimeOffset accessTokenExpiresAt,CancellationToken cancellationToken = default);
}
