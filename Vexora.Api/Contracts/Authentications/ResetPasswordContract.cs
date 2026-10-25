namespace Vexora.Api.Contracts.Authentications;

public sealed record ResetPasswordContract(string PhoneNumber, string OtpCode, string NewPassword);
