using Vexora.Application.Services.Interfaces.CurrentUser;

public sealed class DevelopmentCurrentUser : ICurrentUser
{
    public long UserId => 2;
}