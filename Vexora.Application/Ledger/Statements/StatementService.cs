using Vexora.Application.Ledger.Exceptions;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Services.Interfaces.CurrentUser;

namespace Vexora.Application.Ledger.Statements;

public sealed class StatementService(
    IStatementReader reader,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IStatementService
{
    public const int MaxRows = 5000;

    public async Task<CustomerStatementDto> GetAsync(
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (from is { } fromDate && to is { } toDate && fromDate.Date > toDate.Date)
            throw new LedgerExceptions.InvalidDateRangeException();

        var userId = currentUser.UserId;

        var customerName = await reader.GetCustomerNameAsync(
            userId,
            customerId,
            cancellationToken)
            ?? throw new LedgerExceptions.CustomerNotFoundException();

        var rowsCount = await reader.CountRowsAsync(
            userId,
            customerId,
            from,
            to,
            cancellationToken);

        if (rowsCount > MaxRows)
            throw new LedgerExceptions.StatementTooLargeException();

        var payments = await reader.GetPaymentsAsync(
            userId,
            customerId,
            from,
            to,
            cancellationToken);

        var checks = await reader.GetChecksAsync(
            userId,
            customerId,
            from,
            to,
            cancellationToken);

        return StatementBuilder.Build(
            customerId,
            customerName,
            from,
            to,
            payments,
            checks,
            timeProvider.GetUtcNow());
    }
}
