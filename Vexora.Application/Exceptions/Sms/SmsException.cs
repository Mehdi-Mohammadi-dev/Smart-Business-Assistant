namespace Vexora.Application.Exceptions.Sms;

public sealed class SmsException(long providerCode, string providerMessage)
    : AppException(
        "SMS_SEND_FAILED",
        "ارسال پیامک با مشکل مواجه شد. لطفاً دوباره تلاش کنید.",
        AppErrorType.ExternalService)
{
    // Technical details for logs only. They must never be shown to the end user.
    public long ProviderCode { get; } = providerCode;
    public string ProviderMessage { get; } = providerMessage;
}
