using FluentValidation;
using Vexora.Api.Contracts.Authentications;
using Vexora.Application.Common;

namespace Vexora.Api.Validators.Authentications.Registers;

public sealed class RegisterContractValidator : AbstractValidator<RegisterContract>
{
    public RegisterContractValidator()
    {
        RuleFor(x => x.RegistrationToken)
            .NotEmpty().WithMessage("توکن الزامی است.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("کد تایید الزامی است.")
            .Must(code => System.Text.RegularExpressions.Regex.IsMatch(DigitNormalizer.Normalize(code), @"^\d{6}$"))
            .WithMessage("کد تایید باید ۶ رقم باشد.");
    }
}
