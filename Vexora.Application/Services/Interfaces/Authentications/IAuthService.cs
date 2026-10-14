using Vexora.Application.Dtos.Authentications;

namespace Vexora.Application.Services.Interfaces.Authentications;

public interface IAuthService
{
    Task<SendOtpResultDto> SendOtpAsync(SendOtpDto dto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> RefreshAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);

    Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken = default);
}
