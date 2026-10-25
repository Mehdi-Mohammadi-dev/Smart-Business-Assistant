using Vexora.Application.Common;
using Vexora.Application.Ledger.Exceptions;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Repositories.Checks;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Application.Ledger.Checks;

public sealed class LedgerCheckService(
    ILedgerCheckReader reader,
    ICheckRepository checkRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ILedgerCheckService
{
    public async Task<PagedResult<CheckDto>> ListAsync(
        CheckListQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.DueFrom is { } fromDate && query.DueTo is { } toDate && fromDate.Date > toDate.Date)
            throw new LedgerExceptions.InvalidDateRangeException();

        var today = IranTime.Today(timeProvider);
        var normalized = query with { Search = SearchText.Normalize(query.Search) };

        var page = await reader.ListAsync(
            currentUser.UserId,
            normalized,
            today,
            cancellationToken);

        return new PagedResult<CheckDto>(
            page.Items.Select(x => CheckRules.ToDto(x, today)).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }

    public async Task<CheckDto> GetAsync(
        long checkId,
        CancellationToken cancellationToken = default)
    {
        var row = await reader.GetAsync(
            currentUser.UserId,
            checkId,
            cancellationToken)
            ?? throw new LedgerExceptions.CheckNotFoundException();

        return CheckRules.ToDto(row, IranTime.Today(timeProvider));
    }

    public Task<CheckDto> SettleAsync(
        long checkId,
        DateTimeOffset? at,
        CancellationToken cancellationToken = default)
        => ChangeAsync(
            checkId,
            check => check.MarkAsPaid(at ?? timeProvider.GetUtcNow()),
            cancellationToken);

    public Task<CheckDto> BounceAsync(
        long checkId,
        DateTimeOffset? at,
        CancellationToken cancellationToken = default)
        => ChangeAsync(
            checkId,
            check => check.MarkAsBounced(at ?? timeProvider.GetUtcNow()),
            cancellationToken);

    public Task<CheckDto> RevertAsync(
        long checkId,
        CancellationToken cancellationToken = default)
        => ChangeAsync(
            checkId,
            check => check.RevertToPending(),
            cancellationToken);

    public Task<CheckDto> SetRegistrationAsync(
        long checkId,
        BankRegistrationStatus status,
        CancellationToken cancellationToken = default)
        => ChangeAsync(
            checkId,
            check => check.SetBankRegistration(status),
            cancellationToken);

    public async Task<long> DeleteAsync(
        long checkId,
        CancellationToken cancellationToken = default)
    {
        var check = await FindAsync(checkId, cancellationToken);

        check.SoftDelete();

        await unitOfWork.SaveAsync(cancellationToken);

        return checkId;
    }

    private async Task<CheckDto> ChangeAsync(
        long checkId,
        Action<Check> change,
        CancellationToken cancellationToken)
    {
        var check = await FindAsync(checkId, cancellationToken);

        change(check);

        await unitOfWork.SaveAsync(cancellationToken);

        return await GetAsync(checkId, cancellationToken);
    }

    private async Task<Check> FindAsync(
        long checkId,
        CancellationToken cancellationToken)
        => await checkRepository.GetByIdAsync(
               currentUser.UserId,
               checkId,
               cancellationToken)
           ?? throw new LedgerExceptions.CheckNotFoundException();
}
