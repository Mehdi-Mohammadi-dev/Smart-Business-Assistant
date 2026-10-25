using Microsoft.EntityFrameworkCore;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Checks;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Ledger;

public sealed class LedgerCheckReader(ApplicationDbContext context) : ILedgerCheckReader
{
    public async Task<PagedResult<CheckRow>> ListAsync(
        long userId,
        CheckListQuery query,
        DateTime today,
        CancellationToken cancellationToken = default)
    {
        var filtered = CheckQueries.Filter(
            context.Checks.AsNoTracking(),
            userId,
            query,
            today);

        var total = await filtered.CountAsync(cancellationToken);

        var skip = (query.Page - 1) * query.PageSize;

        var page = CheckQueries
            .Sort(filtered, query.Sort)
            .Skip(skip)
            .Take(query.PageSize);

        var rows = await CheckQueries
            .ToRows(page)
            .ToListAsync(cancellationToken);

        return new PagedResult<CheckRow>(rows, query.Page, query.PageSize, total);
    }

    public Task<CheckRow?> GetAsync(
        long userId,
        long checkId,
        CancellationToken cancellationToken = default)
        => CheckQueries
            .ToRows(context.Checks
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Id == checkId))
            .FirstOrDefaultAsync(cancellationToken);
}
