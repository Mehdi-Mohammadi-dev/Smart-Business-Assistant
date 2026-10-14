namespace Vexora.Application.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Vexora";
    public string Audience { get; set; } = "VexoraClients";

    public string Secret { get; set; } = "";

    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}
