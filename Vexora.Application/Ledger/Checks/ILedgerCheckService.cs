using Vexora.Application.Common;
using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Application.Ledger.Checks;

public interface ILedgerCheckService
{
    Task<PagedResult<CheckDto>> ListAsync(
        CheckListQuery query,
        CancellationToken cancellationToken = default);

    Task<CheckDto> GetAsync(
        long checkId,
        CancellationToken cancellationToken = default);

    Task<CheckDto> SettleAsync(
        long checkId,
        DateTimeOffset? at,
        CancellationToken cancellationToken = default);

    Task<CheckDto> BounceAsync(
        long checkId,
        DateTimeOffset? at,
        CancellationToken cancellationToken = default);

    Task<CheckDto> RevertAsync(
        long checkId,
        CancellationToken cancellationToken = default);

    Task<CheckDto> SetRegistrationAsync(
        long checkId,
        BankRegistrationStatus status,
        CancellationToken cancellationToken = default);

    Task<long> DeleteAsync(
        long checkId,
        CancellationToken cancellationToken = default);
}
