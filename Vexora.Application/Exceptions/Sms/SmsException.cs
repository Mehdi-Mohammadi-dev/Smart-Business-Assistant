namespace Vexora.Application.Exceptions.Sms;

public class SmsException(long code, string msg) : Exception(msg)
{
    public long Code { get; } = code;
}
