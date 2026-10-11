namespace Vexora.Application.Services.Interfaces.Authentications;

public interface ISmsSender
{
    Task<long> SendAsync(string mobile, string text);
}
