namespace Vexora.Domain.Enums.Authentications;

public enum OtpPurpose:byte
{
    PhoneVerification = 1,
    Login = 2,
    PasswordReset = 3
}
