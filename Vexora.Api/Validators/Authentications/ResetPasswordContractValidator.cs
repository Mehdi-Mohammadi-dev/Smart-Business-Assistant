using FluentValidation;
using Vexora.Api.Contracts.Authentications;
using Vexora.Application.Common;

namespace Vexora.Api.Validators.Authentications;

public sealed class ResetPasswordContractValidator : AbstractValidator<ResetPasswordContract>
{
    public ResetPasswordContractValidator()
    {
        RuleFor(x => x.PhoneNumber).Must(phone =>
            System.Text.RegularExpressions.Regex.IsMatch(PhoneNumberNormalizer.Normalize(phone), AuthRules.PhonePattern))
            .WithMessage(AuthRules.PhoneMessage);
        RuleFor(x => x.OtpCode).Must(code =>
            System.Text.RegularExpressions.Regex.IsMatch(DigitNormalizer.Normalize(code), @"^\d{6}$"))
            .WithMessage("کد تأیید باید ۶ رقم باشد.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .WithMessage("رمز عبور باید حداقل ۸ نویسه باشد.");
    }
}
