using Vexora.Api.Contracts.Authentications.Subscriptions;
using Vexora.Api.Contracts.Authentications.Users;

namespace Vexora.Api.Contracts.Authentications.Profiles;

public record ProfileResponse
(
    UserContract UserContract,
    SubscriptionContract? SubscriptionContract
);
