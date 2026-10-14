using Vexora.Api.Contracts.Authentications;
using Vexora.Api.Contracts.Authentications.Profiles;
using Vexora.Api.Contracts.Authentications.Subscriptions;
using Vexora.Api.Contracts.Authentications.Users;
using Vexora.Application.Dtos.Authentications;

namespace Vexora.Api.Mappers.Authentications;

public static class AuthenticationMapper
{
    public static SendOtpDto SendOtpContractMapToSendOtpDto
        (this SendOtpContract contract)
    =>   new SendOtpDto
            (
            contract.FirstName,
            contract.LastName,
            contract.PhoneNumber,
            contract.Password
            );

    public static RegisterDto RegisterContractMapToRegisterDto
        (this RegisterContract contract) =>
        new RegisterDto
        (
            contract.RegistrationToken,
            contract.OtpCode
        );

    public static LoginDto LoginContractMapToLoginDto
        (this LoginContract contract) =>
        new LoginDto
        (
            contract.PhoneNumber,
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
            contract.PhoneNumber
        );
        
    public static UserContract UserDtoMapToUserContract(this UserDto dto) =>
    new UserContract
    (
        dto.Id,
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber
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
         dto.AccessTokenExpiresAt,
         dto.Profile.ProfileContractMapToProfileDto()
            );
}
