using FluentValidation;
using Vexora.Api.Contracts.Authentications;

namespace Vexora.Api.Validators.Authentications.Logins;

public sealed class LoginContractValidator : AbstractValidator<LoginContract>
{
    public LoginContractValidator()
    {
        RuleFor(x => x.PhoneNumber).Phone();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است.");
    }
}
