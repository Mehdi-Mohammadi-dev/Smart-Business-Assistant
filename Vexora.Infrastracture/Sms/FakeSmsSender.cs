using Microsoft.Extensions.Logging;
using Vexora.Application.Services.Interfaces.Authentications;

namespace Vexora.Infrastracture.Sms;

public sealed class FakeSmsSender(ILogger<FakeSmsSender> logger) : ISmsSender
{
    public Task<long> SendAsync(string mobile, string text)
    {
        logger.LogWarning("FAKE SMS to {Mobile}: {Text}", mobile, text);
        return Task.FromResult(1L);
    }
}
