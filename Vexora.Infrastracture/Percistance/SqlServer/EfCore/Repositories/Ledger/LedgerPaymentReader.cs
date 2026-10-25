using Microsoft.EntityFrameworkCore;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Payments;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Ledger;

public sealed class LedgerPaymentReader(ApplicationDbContext context) : ILedgerPaymentReader
{
    public async Task<PagedResult<PaymentRow>> ListAsync(
        long userId,
        PaymentListQuery query,
        CancellationToken cancellationToken = default)
    {
        var filtered = PaymentQueries.Filter(
            context.Payments.AsNoTracking(),
            userId,
            query);

        var total = await filtered.CountAsync(cancellationToken);

        var skip = (query.Page - 1) * query.PageSize;

        var page = filtered
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(query.PageSize);

        var rows = await PaymentQueries
            .ToRows(page)
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentRow>(rows, query.Page, query.PageSize, total);
    }
}
