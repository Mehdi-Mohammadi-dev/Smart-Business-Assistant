using System.Security.Cryptography;
using Vexora.Application.Caching.Authentications;
using Vexora.Application.Dtos.Authentications;
using Vexora.Application.Dtos.Authentications.Otps;
using Vexora.Application.Dtos.Authentications.Profiles;
using Vexora.Application.Dtos.Authentications.Subscriptions;
using Vexora.Application.Dtos.Authentications.Users;
using Vexora.Application.Exceptions.Authentications;
using Vexora.Application.Repositories.Authentications;
using Vexora.Application.Repositories.RefreshTokens;
using Vexora.Application.Repositories.Subscriptions;
using Vexora.Application.Repositories.Users;
using Vexora.Application.Services.Interfaces.Authentications;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Authentications;
using Vexora.Domain.Entities.RefreshTokens;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Domain.Entities.Users;
using Vexora.Domain.Enums.Authentications;

namespace Vexora.Application.Services.Implementations.Authentications;

public sealed class AuthService(
    IUserRepository userRepository,
    IOtpVerificationRepository otpRepository,
    ISubscriptionRepository subscriptionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ISmsSender smsSender,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    IPendingRegistrationStore pendingRegistrationStore,
    IAccessTokenRevocationStore accessTokenRevocationStore) : IAuthService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
    private const int MaxOtpAttempts = 5;

    public async Task<SendOtpResultDto> SendOtpAsync(
    SendOtpDto dto,
    CancellationToken cancellationToken = default)
    {
        var phone = dto.PhoneNumber.Trim();

        if (await userRepository.ExistsByPhoneNumberAsync(
                phone, cancellationToken))
        {
            throw new AuthExceptions.PhoneNumberAlreadyExistsException();
        }

        var now = DateTimeOffset.UtcNow;

        var latest = await otpRepository.GetLatestUnusedAsync(
            phone,
            OtpPurpose.PhoneVerification,
            cancellationToken);

        if (latest is not null && latest.CreateAt > now - OtpResendCooldown)
        {
            throw new AuthExceptions.OtpRequestedTooSoonException();
        }

        var code = RandomNumberGenerator
            .GetInt32(100_000, 1_000_000)
            .ToString();

        var otp = OtpVerification.Create(
            phone,
            passwordHasher.Hash(code),
            OtpPurpose.PhoneVerification,
            now.Add(OtpLifetime));

        await otpRepository.AddAsync(otp, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        var registrationToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        Console.WriteLine("registrationToken : " + registrationToken);

        var registrationLifetime = TimeSpan.FromMinutes(10);

        await pendingRegistrationStore.StoreAsync(
            registrationToken,
            new PendingRegistration
            (
                 otp.Id,
                 dto.FirstName,
                 dto.LastName,
                 phone,
                 passwordHasher.Hash(dto.Password)
            ),
            registrationLifetime,
            cancellationToken);

        await smsSender.SendAsync(
            phone,
            $"کد تأیید وکسورا: {code}");




        return new SendOtpResultDto
        (
            registrationToken,
            now.Add(registrationLifetime)
        );
    }


    public async Task<AuthResultDto> RegisterAsync(
     RegisterDto dto,
     CancellationToken cancellationToken = default)
    {
        var pending = await pendingRegistrationStore.GetAsync(
            dto.RegistrationToken,
            cancellationToken)
            ?? throw new AuthExceptions.OtpInvalidException();

        var otp = await otpRepository.GetByIdAsync(
            pending.OtpId,
            cancellationToken)
            ?? throw new AuthExceptions.OtpInvalidException();

        if (otp.PhoneNumber != pending.PhoneNumber ||
            otp.Purpose != OtpPurpose.PhoneVerification ||
            otp.UsedAt.HasValue)
        {
            throw new AuthExceptions.OtpInvalidException();
        }

        if (otp.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new AuthExceptions.OtpExpiredException();

        if (otp.AttemptCount >= MaxOtpAttempts)
            throw new AuthExceptions.OtpTooManyAttemptsException();

        if (!passwordHasher.Verify(dto.OtpCode.Trim(), otp.CodeHash))
        {
            otp.RegisterFailedAttempt();
            await unitOfWork.SaveAsync(cancellationToken);

            throw new AuthExceptions.OtpInvalidException();
        }

        if (await userRepository.ExistsByPhoneNumberAsync(
                pending.PhoneNumber, cancellationToken))
        {
            throw new AuthExceptions.PhoneNumberAlreadyExistsException();
        }

        otp.MarkAsUsed();

        var user = User.Register(
            pending.FirstName,
            pending.LastName,
            pending.PhoneNumber,
            pending.PasswordHash);

        AuthResultDto? result = null;

        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await userRepository.InsertAsync(user, token);
            await unitOfWork.SaveAsync(token);

            var trial = Subscription.CreateTrial(user.Id);
            await subscriptionRepository.AddAsync(trial, token);

            result = await IssueTokensAsync(
                user,
                trial,
                rotateFrom: null,
                token);

            await unitOfWork.SaveAsync(token);
        }, cancellationToken);

        await pendingRegistrationStore.RemoveAsync(
            dto.RegistrationToken,
            cancellationToken);

        return result!;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByPhoneNumberAsync(dto.PhoneNumber.Trim(), cancellationToken);

        if (user is null || !passwordHasher.Verify(dto.Password, user.PasswordHasher))
            throw new AuthExceptions.InvalidCredentialsException();

        var subscription = await subscriptionRepository.GetLatestByUserIdAsync(user.Id, cancellationToken);

        return await IssueTokensAsync(user, subscription, rotateFrom: null, cancellationToken);
    }

    public async Task<AuthResultDto> RefreshAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashRefreshToken(dto.RefreshToken);

        var stored = await refreshTokenRepository.GetByHashAsync(hash, cancellationToken);

        if (stored is null || stored.RevokedAt.HasValue || stored.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new AuthExceptions.InvalidRefreshTokenException();

        var user = await userRepository.FindByIdAsync(stored.UserId, cancellationToken)
                   ?? throw new AuthExceptions.InvalidRefreshTokenException();

        var subscription = await subscriptionRepository.GetLatestByUserIdAsync(user.Id, cancellationToken);

        return await IssueTokensAsync(user, subscription, rotateFrom: stored, cancellationToken);
    }

    public async Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(currentUser.UserId, cancellationToken)
                   ?? throw new AuthExceptions.UnauthorizedException();

        var subscription = await subscriptionRepository.GetLatestByUserIdAsync(user.Id, cancellationToken);

        return ToProfile(user, subscription);
    }


    private async Task<AuthResultDto> IssueTokensAsync(
        User user,
        Subscription? subscription,
        RefreshToken? rotateFrom,
        CancellationToken cancellationToken)
    {
        var access = tokenService.CreateAccessToken(user);

        var refresh = tokenService.GenerateRefreshToken();
        var refreshHash = tokenService.HashRefreshToken(refresh);
        var refreshExpiresAt = DateTimeOffset.UtcNow.Add(tokenService.RefreshTokenLifetime);

        rotateFrom?.Revoke(refreshHash);

        await refreshTokenRepository.AddAsync(
            RefreshToken.Create(user.Id, refreshHash, refreshExpiresAt),
            cancellationToken);

        await unitOfWork.SaveAsync(cancellationToken);

        return new AuthResultDto(
            access.Token,
            access.ExpiresAt,
            refresh,
            refreshExpiresAt,
            ToProfile(user, subscription));
    }

    private static ProfileDto ToProfile(User user, Subscription? subscription)
    {
        var now = DateTimeOffset.UtcNow;

        SubscriptionDto? subscriptionDto = null;

        if (subscription is not null)
        {
            var isActive = subscription.IsActiveAt(now);

            subscriptionDto = new SubscriptionDto(
                subscription.PlanType.ToString(),
                subscription.Status.ToString(),
                subscription.StartAt,
                subscription.EndAt,
                isActive,
                isActive ? (int)Math.Ceiling((subscription.EndAt - now).TotalDays) : 0);
        }

        return new ProfileDto(
            new UserDto(user.Id, user.FirstName, user.LastName, user.PhoneNumber),
            subscriptionDto);
    }

    public async Task LogoutAsync(
    RefreshTokenDto dto,
    string accessTokenId,
    DateTimeOffset accessTokenExpiresAt,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessTokenId))
            throw new AuthExceptions.UnauthorizedException();

        var hash = tokenService.HashRefreshToken(dto.RefreshToken);

        var stored = await refreshTokenRepository.GetByHashAsync(
            hash,
            cancellationToken);

        if (stored is null ||
            stored.UserId != currentUser.UserId ||
            stored.RevokedAt.HasValue ||
            stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new AuthExceptions.InvalidRefreshTokenException();
        }

        stored.Revoke();

        await unitOfWork.SaveAsync(cancellationToken);

        await accessTokenRevocationStore.RevokeAsync(
            accessTokenId,
            accessTokenExpiresAt,
            cancellationToken);
    }
}
