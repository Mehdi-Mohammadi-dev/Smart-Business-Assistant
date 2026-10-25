using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Filters;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Ledger;
using Vexora.Api.Mappers.Ledger;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Payments;

namespace Vexora.Api.Controllers.Ledger;

[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(ILedgerPaymentService paymentService)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("فهرست پرداخت‌ها")]
    [EndpointDescription("پرداخت‌های ثبت‌شده، جدیدترین اول. فیلتر: customerId، from، to. pageSize حداکثر ۱۰۰.")]
    [ProducesResponseType<ApiResult<PagedResult<PaymentResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<PagedResult<PaymentResponse>>> List(
        [FromQuery] PaymentListRequest request,
        CancellationToken cancellationToken)
    {
        var page = await paymentService.ListAsync(request.ToQuery(), cancellationToken);

        return ApiResult<PagedResult<PaymentResponse>>.SuccessResult(
            page.Map(x => x.ToResponse()));
    }

    [HttpDelete("{id:long}")]
    [RequireActiveSubscription]
    [EndpointSummary("ابطال پرداخت")]
    [EndpointDescription("پرداخت اشتباه را باطل می‌کند (حذف نرم). از همه جمع‌ها و لیست‌ها حذف می‌شود.")]
    [ProducesResponseType<ApiResult<DeletedResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ApiResult<DeletedResponse>> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var deletedId = await paymentService.DeleteAsync(id, cancellationToken);

        return ApiResult<DeletedResponse>.SuccessResult(new DeletedResponse(deletedId));
    }
}
