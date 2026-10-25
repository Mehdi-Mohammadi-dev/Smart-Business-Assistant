using Vexora.Domain.Constants.RefreshTokens;

namespace Vexora.Domain.Exceptions.RefreshTokens;

public sealed class RefreshTokenRequired() : Exception
(RefreshTokenConstants.TokenHashRequired);
