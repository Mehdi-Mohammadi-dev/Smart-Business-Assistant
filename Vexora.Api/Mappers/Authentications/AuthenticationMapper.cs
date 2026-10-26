using Vexora.Api.Contracts.Authentications;
using Vexora.Application.Common;
using Vexora.Api.Contracts.Authentications.Profiles;
using Vexora.Api.Contracts.Authentications.Subscriptions;
using Vexora.Api.Contracts.Authentications.Users;
using Vexora.Application.Dtos.Authentications;
using Vexora.Application.Dtos.Authentications.Otps;
using Vexora.Application.Dtos.Authentications.Profiles;
using Vexora.Application.Dtos.Authentications.Subscriptions;
using Vexora.Application.Dtos.Authentications.Users;

namespace Vexora.Api.Mappers.Authentications;

public static class AuthenticationMapper
{
    public static SendOtpDto SendOtpContractMapToSendOtpDto
        (this SendOtpContract contract)
    =>   new SendOtpDto
            (
            contract.FirstName,
            contract.LastName,
            PhoneNumberNormalizer.Normalize(contract.PhoneNumber),
            contract.Password,
            contract.BusinessName
            );

    public static RegisterDto RegisterContractMapToRegisterDto
        (this RegisterContract contract) =>
        new RegisterDto
        (
            contract.RegistrationToken,
            DigitNormalizer.Normalize(contract.OtpCode)
        );

    public static LoginDto LoginContractMapToLoginDto
        (this LoginContract contract) =>
        new LoginDto
        (
            PhoneNumberNormalizer.Normalize(contract.PhoneNumber),
            contract.Password
        );

    public static RefreshTokenDto RefreshTokenContractMapToRefreshTokenDto(this RefreshTokenContract contract) =>
        new RefreshTokenDto
        (
            contract.RefreshToken
        );

    public static ProfileDto ProfileContractMapToProfileDto
        (this ProfileResponse contract) =>
        new ProfileDto
        (
            contract.UserContract.UserContractMapToUserDto(),
            contract.SubscriptionContract?.SubscriptionContractMapToSubscriptionDto()
        );

    public static ProfileResponse ProfileContractMapToProfileDto
        (this ProfileDto dto) =>
        new ProfileResponse
        (
            dto.User.UserDtoMapToUserContract(),
            dto.Subscription?.SubscriptionDtoMapToSubscriptionContract()
        );

    public static SubscriptionDto SubscriptionContractMapToSubscriptionDto(this SubscriptionContract contract) =>
        new SubscriptionDto
        (
            contract.PlanType,
            contract.Status,
            contract.StartAt,
            contract.EndAt,
            contract.IsActive,
            contract.RemainingDays
        );

    public static UserDto UserContractMapToUserDto(this UserContract contract) =>
        new UserDto
        (
            contract.Id,
            contract.FirstName,
            contract.LastName,
            contract.PhoneNumber,
            contract.BusinessName
        );

    public static UserContract UserDtoMapToUserContract(this UserDto dto) =>
    new UserContract
    (
        dto.Id,
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber,
        dto.BusinessName
    );

    public static SubscriptionContract SubscriptionDtoMapToSubscriptionContract(this     SubscriptionDto dto) =>
        new SubscriptionContract
        (
            dto.PlanType,
            dto.Status,
            dto.StartAt,
            dto.EndAt,
            dto.IsActive,
            dto.RemainingDays
        );

    public static AuthResultResponse AuthResultDtoMapToAuthResultContract(this AuthResultDto dto)
        =>
        new AuthResultResponse
        (
         dto.AccessToken,
         dto.AccessTokenExpiresAt,
         dto.RefreshToken,
         dto.RefreshTokenExpiresAt,
         dto.Profile.ProfileContractMapToProfileDto()
            );
}
