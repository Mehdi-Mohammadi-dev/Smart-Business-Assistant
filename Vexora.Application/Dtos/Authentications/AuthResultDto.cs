namespace Vexora.Application.Dtos.Authentications;

public record UserDto(
    long Id,
    string FirstName,
    string LastName,
    string PhoneNumber);

public record SubscriptionDto(
    string PlanType,
    string Status,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsActive,
    int RemainingDays);

public record ProfileDto(
    UserDto User,
    SubscriptionDto? Subscription);

public record AuthResultDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    ProfileDto Profile);
