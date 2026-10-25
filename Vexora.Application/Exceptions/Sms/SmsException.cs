using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Application.Exceptions.Sms;

public sealed class SmsException(long providerCode, string providerMessage)
    : AppException(
        "SMS_SEND_FAILED",
        "ارسال پیامک با مشکل مواجه شد. لطفاً دوباره تلاش کنید.",
        AppErrorType.ExternalService)
{
    public long ProviderCode { get; } = providerCode;
    public string ProviderMessage { get; } = providerMessage;
}
