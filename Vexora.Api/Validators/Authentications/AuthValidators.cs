using FluentValidation;

namespace Vexora.Api.Validators.Authentications;

public static class AuthRules
{
    public const string PhonePattern = @"^09\d{9}$";
    public const string PhoneMessage = "شماره موبایل معتبر نیست (مثال: 09121234567).";

    public static IRuleBuilderOptions<T, string> Phone<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Matches(PhonePattern).WithMessage(PhoneMessage);
}

