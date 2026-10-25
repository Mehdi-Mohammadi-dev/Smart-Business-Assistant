using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Application.Platform.Statements;

namespace Vexora.Api.Controllers.Reports;

[ApiController]
[Authorize]
[Route("api/customers/{id:long}/statement.pdf")]
public sealed class CustomerStatementController(
    IStatementService statementService,
    IStatementPdfGenerator pdfGenerator) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("دریافت PDF گردش حساب مشتری")]
    [EndpointDescription("گزارش گردش دریافت‌های مشتری را به‌صورت فایل PDF دریافت می‌کند.")]
    [Produces("application/pdf", "application/json")]
    public async Task<IActionResult> Get(long id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var statement = await statementService.GetAsync(id, from, to, cancellationToken);
        var bytes = pdfGenerator.Generate(statement);
        return File(bytes, "application/pdf", $"statement-{id}-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
