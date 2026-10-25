using Vexora.Domain.Constants.RefreshTokens;

namespace Vexora.Domain.Exceptions.RefreshTokens;

public sealed class InvalidExpiresDate() : Exception
(RefreshTokenConstants.ExpiresDate);
