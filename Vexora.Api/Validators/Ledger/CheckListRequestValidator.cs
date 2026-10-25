using FluentValidation;
using Vexora.Api.Constants.Ledger;
using Vexora.Api.Contracts.Ledger;

namespace Vexora.Api.Validators.Ledger;

public sealed class CheckListRequestValidator : AbstractValidator<CheckListRequest>
{
    public CheckListRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(LedgerMessages.PageInvalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(LedgerMessages.PageSizeInvalid);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .When(x => x.CustomerId.HasValue)
            .WithMessage(LedgerMessages.IdInvalid);

        RuleFor(x => x.Settlement)
            .IsInEnum()
            .When(x => x.Settlement.HasValue)
            .WithMessage(LedgerMessages.ValueInvalid);

        RuleFor(x => x.Registration)
            .IsInEnum()
            .When(x => x.Registration.HasValue)
            .WithMessage(LedgerMessages.ValueInvalid);

        RuleFor(x => x.Sort)
            .IsInEnum()
            .WithMessage(LedgerMessages.ValueInvalid);

        RuleFor(x => x.DueWithinDays)
            .InclusiveBetween(0, 3650)
            .When(x => x.DueWithinDays.HasValue)
            .WithMessage(LedgerMessages.DueWithinDaysInvalid);

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(LedgerMessages.SearchTooLong);

        RuleFor(x => x)
            .Must(x => !x.DueFrom.HasValue || !x.DueTo.HasValue || x.DueFrom.Value.Date <= x.DueTo.Value.Date)
            .WithMessage(LedgerMessages.DateRangeInvalid);
    }
}
