using Vexora.Application.Common;
using Vexora.Application.Ledger.Exceptions;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Services.Interfaces.CurrentUser;

namespace Vexora.Application.Ledger.Customers;

public sealed class LedgerCustomerService(
    ILedgerCustomerReader reader,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ILedgerCustomerService
{
    public async Task<PagedResult<CustomerSummaryDto>> ListAsync(
        CustomerListQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var today = IranTime.Today(timeProvider);

        var customers = await reader.FindCustomersAsync(
            userId,
            SearchText.Normalize(query.Search),
            null,
            cancellationToken);

        var payments = await reader.GetPaymentAggregatesAsync(
            userId,
            null,
            cancellationToken);

        var checks = await reader.GetCheckAggregatesAsync(
            userId,
            null,
            today,
            cancellationToken);

        var sorted = CustomerSummaryBuilder.Sort(
            CustomerSummaryBuilder.Build(customers, payments, checks),
            query.Sort);

        var items = sorted
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<CustomerSummaryDto>(
            items,
            query.Page,
            query.PageSize,
            sorted.Count);
    }

    public async Task<CustomerSummaryDto> GetAsync(
        long customerId,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var today = IranTime.Today(timeProvider);

        var customers = await reader.FindCustomersAsync(
            userId,
            null,
            customerId,
            cancellationToken);

        if (customers.Count == 0)
            throw new LedgerExceptions.CustomerNotFoundException();

        var payments = await reader.GetPaymentAggregatesAsync(
            userId,
            customerId,
            cancellationToken);

        var checks = await reader.GetCheckAggregatesAsync(
            userId,
            customerId,
            today,
            cancellationToken);

        return CustomerSummaryBuilder.Build(customers, payments, checks)[0];
    }
}
