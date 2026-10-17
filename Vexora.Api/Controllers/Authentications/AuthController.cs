using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Authentications;
using Vexora.Api.Contracts.Authentications.Profiles;
using Vexora.Api.Mappers.Authentications;
using Vexora.Application.Services.Interfaces.Authentications;

namespace Vexora.Api.Controllers.Authentications;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("otp")]
    [HttpPost("send-otp")]
    public async Task<ApiResult<SendOtpResponse>> SendOtp(
    [FromBody] SendOtpContract request,
    CancellationToken cancellationToken)
    {
        var result = await authService.SendOtpAsync(
            request.SendOtpContractMapToSendOtpDto(), cancellationToken);

        return ApiResult<SendOtpResponse>.SuccessResult(
            new SendOtpResponse(result.RegistrationToken, result.ExpiresAt),
            "کد تایید ارسال شد.");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ApiResult<AuthResultResponse>> Register(
        [FromBody] RegisterContract request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.RegisterContractMapToRegisterDto(), cancellationToken);

        return ApiResult<AuthResultResponse>.SuccessResult(result.AuthResultDtoMapToAuthResultContract(), "ثبت‌نام با موفقیت انجام شد.");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ApiResult<AuthResultResponse>> Login(
        [FromBody] LoginContract request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.LoginContractMapToLoginDto(), cancellationToken);

        return ApiResult<AuthResultResponse>.SuccessResult(result.AuthResultDtoMapToAuthResultContract(), "ورود با موفقیت انجام شد.");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ApiResult<AuthResultResponse>> Refresh(
        [FromBody] RefreshTokenContract request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshTokenContractMapToRefreshTokenDto(), cancellationToken);

        return ApiResult<AuthResultResponse>.SuccessResult(result.AuthResultDtoMapToAuthResultContract());
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ApiResult<ProfileResponse>> Me(CancellationToken cancellationToken)
    {
        var profile = await authService.GetProfileAsync(cancellationToken);

        return ApiResult<ProfileResponse>.SuccessResult(profile.ProfileContractMapToProfileDto());
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ApiResult<object?>> Logout(
    [FromBody] RefreshTokenContract request,
    CancellationToken cancellationToken)
    {
        var tokenId = User.FindFirst("jti")?.Value;
        var expiresAtClaim = User.FindFirst("exp")?.Value;

        if (string.IsNullOrWhiteSpace(tokenId) ||
            !long.TryParse(expiresAtClaim, out var expiresAtUnix))
        {
            throw new Vexora.Application.Exceptions.Authentications
                .AuthExceptions.UnauthorizedException();
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix);

        await authService.LogoutAsync(
            request.RefreshTokenContractMapToRefreshTokenDto(),
            tokenId,
            expiresAt,
            cancellationToken);

        return ApiResult<object?>.SuccessResult(
            null,
            "با موفقیت از حساب کاربری خارج شدید.");
    }
}
