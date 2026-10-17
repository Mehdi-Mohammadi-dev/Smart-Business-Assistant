using Vexora.Application.Dtos.Authentications.Subscriptions;
using Vexora.Application.Dtos.Authentications.Users;

namespace Vexora.Application.Dtos.Authentications.Profiles;

public record ProfileDto(
    UserDto User,
    SubscriptionDto? Subscription);
