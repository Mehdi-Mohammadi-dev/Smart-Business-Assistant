using Vexora.Domain.Constants.Authentications;

namespace Vexora.Domain.Exceptions.Authentications;

public sealed class PhoneNumberRequired() : Exception
(AuthenticationConstants.PhoneNumberRequired);
