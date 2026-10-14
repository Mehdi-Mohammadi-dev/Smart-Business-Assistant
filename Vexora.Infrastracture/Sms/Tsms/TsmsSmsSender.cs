using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vexora.Application.Exceptions.Sms;
using Vexora.Application.Services.Interfaces.Authentications;
using Vexora.Infrastracture.Sms.Tsms.TsmsClient;

namespace Vexora.Infrastracture.Sms.Tsms;

public sealed class TsmsSmsSender(
    IOptions<TsmsOptions> options,
    ILogger<TsmsSmsSender> logger) : ISmsSender
{
    private readonly TsmsOptions _opt = options.Value;

    public async Task<long> SendAsync(string mobile, string text)
    {
        var client = new tsmsServiceClient();
        client.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(20);

        int[] result;

        try
        {
            result = await client.sendSmsAsync(
                _opt.Username,
                _opt.Password,
                new[] { _opt.SmsNumber },
                new[] { mobile },
                new[] { text },
                new[] { "0" },
                Guid.NewGuid().ToString("N"));

            client.Close();
        }
        catch (Exception ex)
        {
            client.Abort();
            logger.LogError(ex, "TSMS request failed");
            throw new SmsException(-1, "Could not reach the SMS service.");
        }

        if (result is null || result.Length == 0)
        {
            logger.LogError("TSMS returned an empty response");
            throw new SmsException(-1, "Empty response from the SMS service.");
        }

        var code = result[0];

        if (code < 0)
        {
            var message = Describe(code);
            logger.LogError("TSMS error {Code}: {Message}", code, message);
            throw new SmsException(code, message);
        }

        return code;
    }

    private static string Describe(int c) => c switch
    {
        -8 => "Wrong username or password",
        -14 => "Not enough credit",
        -6 => "Invalid mobile number",
        -17 => "Send limit exceeded",
        -24 => "Message did not reach the server",
        _ => "Unknown error"
    };
}
