using FluentValidation;
using Vexora.Api.Contracts.Authentications;

namespace Vexora.Api.Validators.Authentications.RefreshTokens;

public sealed class RefreshTokenContractValidator : AbstractValidator<RefreshTokenContract>
{
    public RefreshTokenContractValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("توکن نشست الزامی است.");
    }
}
