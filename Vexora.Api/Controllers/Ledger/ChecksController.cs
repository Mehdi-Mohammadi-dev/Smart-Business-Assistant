using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Vexora.Api.Commons.Filters;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Ledger;
using Vexora.Api.Mappers.Ledger;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Checks;

namespace Vexora.Api.Controllers.Ledger;

[Authorize]
[ApiController]
[Route("api/checks")]
public sealed class ChecksController(ILedgerCheckService checkService)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("فهرست چک‌ها")]
    [EndpointDescription("فیلتر: customerId، settlement، registration، dueFrom، dueTo، overdue، dueWithinDays، search. sort: DueDate (پیش‌فرض، صعودی) یا CreatedAt (جدیدترین اول).")]
    [ProducesResponseType<ApiResult<PagedResult<CheckResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<PagedResult<CheckResponse>>> List(
        [FromQuery] CheckListRequest request,
        CancellationToken cancellationToken)
    {
        var page = await checkService.ListAsync(request.ToQuery(), cancellationToken);

        return ApiResult<PagedResult<CheckResponse>>.SuccessResult(
            page.Map(x => x.ToResponse()));
    }

    [HttpGet("{id:long}")]
    [EndpointSummary("یک چک")]
    [EndpointDescription("جزئیات یک چک همراه وضعیت و روز تا سررسید.")]
    [ProducesResponseType<ApiResult<CheckResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ApiResult<CheckResponse>> Get(
        long id,
        CancellationToken cancellationToken)
    {
        var check = await checkService.GetAsync(id, cancellationToken);

        return ApiResult<CheckResponse>.SuccessResult(check.ToResponse());
    }

    [HttpPost("{id:long}/settle")]
    [RequireActiveSubscription]
    [EndpointSummary("وصول شد")]
    [EndpointDescription("چک در انتظار را وصول‌شده می‌کند. بدنه اختیاری است: at (پیش‌فرض الان).")]
    [ProducesResponseType<ApiResult<CheckResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status409Conflict)]
    public async Task<ApiResult<CheckResponse>> Settle(
        long id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CheckActionRequest? request,
        CancellationToken cancellationToken)
    {
        var check = await checkService.SettleAsync(id, request?.At, cancellationToken);

        return ApiResult<CheckResponse>.SuccessResult(check.ToResponse());
    }

    [HttpPost("{id:long}/bounce")]
    [RequireActiveSubscription]
    [EndpointSummary("برگشت خورد")]
    [EndpointDescription("چک در انتظار را برگشتی می‌کند. بدنه اختیاری است: at (پیش‌فرض الان).")]
    [ProducesResponseType<ApiResult<CheckResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status409Conflict)]
    public async Task<ApiResult<CheckResponse>> Bounce(
        long id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CheckActionRequest? request,
        CancellationToken cancellationToken)
    {
        var check = await checkService.BounceAsync(id, request?.At, cancellationToken);

        return ApiResult<CheckResponse>.SuccessResult(check.ToResponse());
    }

    [HttpPost("{id:long}/revert")]
    [RequireActiveSubscription]
    [EndpointSummary("بازگشت به در انتظار")]
    [EndpointDescription("چک وصول‌شده یا برگشتی را دوباره در انتظار می‌کند.")]
    [ProducesResponseType<ApiResult<CheckResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status409Conflict)]
    public async Task<ApiResult<CheckResponse>> Revert(
        long id,
        CancellationToken cancellationToken)
    {
        var check = await checkService.RevertAsync(id, cancellationToken);

        return ApiResult<CheckResponse>.SuccessResult(check.ToResponse());
    }

    [HttpPut("{id:long}/registration")]
    [RequireActiveSubscription]
    [EndpointSummary("ثبت در سامانه صیاد")]
    [EndpointDescription("وضعیت ثبت چک در سامانه صیاد را عوض می‌کند: Registered یا NotRegistered.")]
    [ProducesResponseType<ApiResult<CheckResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ApiResult<CheckResponse>> SetRegistration(
        long id,
        [FromBody] CheckRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var check = await checkService.SetRegistrationAsync(
            id,
            request.BankRegistration,
            cancellationToken);

        return ApiResult<CheckResponse>.SuccessResult(check.ToResponse());
    }

    [HttpDelete("{id:long}")]
    [RequireActiveSubscription]
    [EndpointSummary("ابطال چک")]
    [EndpointDescription("چک اشتباه را باطل می‌کند (حذف نرم). از همه جمع‌ها و لیست‌ها حذف می‌شود.")]
    [ProducesResponseType<ApiResult<DeletedResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ApiResult<DeletedResponse>> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var deletedId = await checkService.DeleteAsync(id, cancellationToken);

        return ApiResult<DeletedResponse>.SuccessResult(new DeletedResponse(deletedId));
    }
}
