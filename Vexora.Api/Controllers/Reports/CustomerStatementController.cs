using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Application.Repositories.Users;
using Vexora.Application.Services.Interfaces.CurrentUser;
using LedgerStatements = Vexora.Application.Ledger.Statements;
using PlatformStatements = Vexora.Application.Platform.Statements;

namespace Vexora.Api.Controllers.Reports;

[ApiController]
[Authorize]
[Route("api/customers/{id:long}/statement")]
public sealed class CustomerStatementController(
    LedgerStatements.IStatementService statementService,
    PlatformStatements.IStatementPdfGenerator pdfGenerator,
    IUserRepository users,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("pdf")]
    [EndpointSummary("دریافت PDF گردش حساب مشتری")]
    [EndpointDescription("گزارش گردش دریافت‌های مشتری را به‌صورت فایل PDF دریافت می‌کند. نام کسب‌وکار در سربرگ نمایش داده می‌شود.")]
    [Produces("application/pdf", "application/json")]
    public async Task<IActionResult> Get(long id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var s = await statementService.GetAsync(id, from, to, cancellationToken);
        var user = await users.FindByIdAsync(currentUser.UserId, cancellationToken);

        var statement = new PlatformStatements.CustomerStatementDto(
            s.CustomerId,
            s.CustomerName,
            s.From,
            s.To,
            new PlatformStatements.StatementTotalsDto(
                s.Totals.CashReceived,
                s.Totals.ChecksPending,
                s.Totals.ChecksPaid,
                s.Totals.ChecksBounced,
                s.Totals.TotalReceived,
                s.Totals.PaymentsCount,
                s.Totals.ChecksCount),
            s.Rows.Select(r => new PlatformStatements.StatementRowDto(
                r.Type, r.Id, r.Date, r.DatePersian, r.Amount, r.Status,
                r.CheckNumber, r.DueDate, r.DueDatePersian, r.BankRegistration)).ToList(),
            s.GeneratedAt,
            user?.DisplayName);

        var bytes = pdfGenerator.Generate(statement);
        return File(bytes, "application/pdf", $"statement-{id}-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
