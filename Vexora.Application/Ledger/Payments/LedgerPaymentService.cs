using Vexora.Application.Common;
using Vexora.Application.Ledger.Exceptions;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Repositories.Payments;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;

namespace Vexora.Application.Ledger.Payments;

public sealed class LedgerPaymentService(
    ILedgerPaymentReader reader,
    IPaymentRepository paymentRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ILedgerPaymentService
{
    public async Task<PagedResult<PaymentDto>> ListAsync(
        PaymentListQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.From is { } fromDate && query.To is { } toDate && fromDate.Date > toDate.Date)
            throw new InvalidDateRangeException();

        var page = await reader.ListAsync(
            currentUser.UserId,
            query,
            cancellationToken);

        return new PagedResult<PaymentDto>(
            page.Items.Select(ToDto).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }

    public async Task<long> DeleteAsync(
        long paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(
            currentUser.UserId,
            paymentId,
            cancellationToken)
            ?? throw new PaymentNotFoundException();

        payment.SoftDelete();

        await unitOfWork.SaveAsync(cancellationToken);

        return paymentId;
    }

    private static PaymentDto ToDto(PaymentRow row)
        => new(
            row.Id,
            row.CustomerId,
            PersonName.Full(row.CustomerFirstName, row.CustomerLastName),
            row.Amount,
            row.PaymentDate,
            row.Direction);
}
