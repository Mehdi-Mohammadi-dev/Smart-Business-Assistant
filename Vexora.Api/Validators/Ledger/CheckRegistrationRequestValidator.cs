using FluentValidation;
using Vexora.Api.Constants.Ledger;
using Vexora.Api.Contracts.Ledger;

namespace Vexora.Api.Validators.Ledger;

public sealed class CheckRegistrationRequestValidator : AbstractValidator<CheckRegistrationRequest>
{
    public CheckRegistrationRequestValidator()
    {
        RuleFor(x => x.BankRegistration)
            .IsInEnum()
            .WithMessage(LedgerMessages.ValueInvalid);
    }
}
