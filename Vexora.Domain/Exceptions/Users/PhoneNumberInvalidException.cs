using Vexora.Domain.Constants.Users;

namespace Vexora.Domain.Exceptions.Users;

public sealed class PhoneNumberInvalidException() : Exception
(UserConstants.PhoneNumberInvalid);
