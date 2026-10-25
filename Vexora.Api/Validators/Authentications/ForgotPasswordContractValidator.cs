using FluentValidation;
using Vexora.Api.Contracts.Authentications;
using Vexora.Api.Validators.Authentications;

namespace Vexora.Api.Validators.Authentications;

public sealed class ForgotPasswordContractValidator : AbstractValidator<ForgotPasswordContract>
{
    public ForgotPasswordContractValidator()
    {
        RuleFor(x => x.PhoneNumber).Phone();
    }
}
