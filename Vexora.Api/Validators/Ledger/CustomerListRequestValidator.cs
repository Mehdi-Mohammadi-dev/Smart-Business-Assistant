using FluentValidation;
using Vexora.Api.Constants.Ledger;
using Vexora.Api.Contracts.Ledger;

namespace Vexora.Api.Validators.Ledger;

public sealed class CustomerListRequestValidator : AbstractValidator<CustomerListRequest>
{
    public CustomerListRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(LedgerMessages.PageInvalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(LedgerMessages.PageSizeInvalid);

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(LedgerMessages.SearchTooLong);

        RuleFor(x => x.Sort)
            .IsInEnum()
            .WithMessage(LedgerMessages.ValueInvalid);
    }
}
