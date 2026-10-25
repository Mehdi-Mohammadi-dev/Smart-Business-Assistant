using FluentValidation;
using Vexora.Api.Constants.Ledger;
using Vexora.Api.Contracts.Ledger;

namespace Vexora.Api.Validators.Ledger;

public sealed class PaymentListRequestValidator : AbstractValidator<PaymentListRequest>
{
    public PaymentListRequestValidator()
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

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value.Date <= x.To.Value.Date)
            .WithMessage(LedgerMessages.DateRangeInvalid);
    }
}
