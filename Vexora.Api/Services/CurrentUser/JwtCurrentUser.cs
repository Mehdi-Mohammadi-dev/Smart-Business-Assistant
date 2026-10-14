using System.Security.Claims;
using Vexora.Application.Exceptions.Authentications;
using Vexora.Application.Services.Interfaces.CurrentUser;

namespace Vexora.Api.Services.CurrentUser;

public sealed class JwtCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public long UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            var value = user?.FindFirst("sub")?.Value
                        ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(value, out var id) || id <= 0)
                throw new AuthExceptions.UnauthorizedException();

            return id;
        }
    }
}
