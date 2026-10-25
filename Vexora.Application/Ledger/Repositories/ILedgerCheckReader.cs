using Vexora.Application.Common;
using Vexora.Application.Ledger.Checks;
using Vexora.Application.Ledger.Models;

namespace Vexora.Application.Ledger.Repositories;

public interface ILedgerCheckReader
{
    Task<PagedResult<CheckRow>> ListAsync(
        long userId,
        CheckListQuery query,
        DateTime today,
        CancellationToken cancellationToken = default);

    Task<CheckRow?> GetAsync(
        long userId,
        long checkId,
        CancellationToken cancellationToken = default);
}
