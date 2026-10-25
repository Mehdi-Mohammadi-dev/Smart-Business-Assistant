namespace Vexora.Application.Ledger.Summary;

public interface ILedgerSummaryService
{
    Task<LedgerSummaryDto> GetAsync(CancellationToken cancellationToken = default);
}
