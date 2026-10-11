using Microsoft.Extensions.Options;
using Vexora.Application.Exceptions.Sms;
using Vexora.Application.Services.Interfaces.Authentications;
using Vexora.Infrastracture.Sms.Tsms.TsmsClient;

namespace Vexora.Infrastracture.Sms.Tsms;

public class TsmsSmsSender : ISmsSender
{
    private readonly TsmsOptions _opt;
    public TsmsSmsSender(IOptions<TsmsOptions> opt) => _opt = opt.Value;

    public async Task<long> SendAsync(string mobile, string text)
    {
        var client = new tsmsServiceClient();
        try
        {
            var messageId = Guid.NewGuid().ToString("N");

            int[] result = await client.sendSmsAsync(
                _opt.Username,
                _opt.Password,
                new[] { _opt.SmsNumber },
                new[] { mobile },
                new[] { text },
                new[] { "0" },
                messageId);

            if (result is null || result.Length == 0)
                throw new SmsException(-1, "پاسخ خالی از سرویس پیامک");

            var code = result[0];
            if (code < 0) throw new SmsException(code, Describe(code));

            client.Close();
            return code;
        }
        catch
        {
            client.Abort();
            throw;
        }
    }

    private static string Describe(int c) => c switch
    {
        -8 => "نام کاربری یا رمز اشتباه است",
        -14 => "اعتبار کافی نیست",
        -6 => "شماره موبایل نامعتبر است",
        -17 => "تعداد ارسال بیش از حد مجاز",
        -24 => "پیام به سرور نرسیده، دوباره ارسال کن",
        _ => "خطای نامشخص"
    };
}