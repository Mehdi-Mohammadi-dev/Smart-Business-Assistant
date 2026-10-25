using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Ledger;
using Vexora.Api.Mappers.Ledger;
using Vexora.Application.Ledger.Summary;

namespace Vexora.Api.Controllers.Ledger;

[Authorize]
[ApiController]
[Route("api/summary")]
public sealed class SummaryController(ILedgerSummaryService summaryService)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("خلاصه داشبورد")]
    [EndpointDescription("تعداد مشتری‌ها، وضعیت چک‌های در انتظار و جمع پرداخت‌های امروز و ماه شمسی جاری (به وقت ایران).")]
    [ProducesResponseType<ApiResult<SummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<SummaryResponse>> Get(CancellationToken cancellationToken)
    {
        var summary = await summaryService.GetAsync(cancellationToken);

        return ApiResult<SummaryResponse>.SuccessResult(summary.ToResponse());
    }
}
