using Vexora.Application.Services.Interfaces.Authentications;

namespace Vexora.Infrastracture.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string value)
        => BCrypt.Net.BCrypt.HashPassword(value, workFactor: 11);

    public bool Verify(string value, string hash)
        => BCrypt.Net.BCrypt.Verify(value, hash);
}
