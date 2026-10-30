using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Referrals;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.SubscriptionPlans;

namespace Vexora.Api.Controllers.Referrals;

[ApiController]
[Route("api/referrals")]
[Authorize]
public sealed class ReferralsController(
    IReferralService referralService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("mine")]
    [EndpointSummary("فهرست کاربران دعوت‌شده توسط من")]
    [EndpointDescription("کاربرانی را نمایش می‌دهد که با کد دعوت این کاربر ثبت‌نام کرده‌اند.")]
    public async Task<ApiResult<IReadOnlyList<MyReferralResponse>>> Mine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new BadHttpRequestException("پارامترهای صفحه‌بندی نامعتبر است.");

        var rows = await referralService.ListByReferrerAsync(
            currentUser.UserId, page, pageSize, cancellationToken);

        var response = rows.Select(x => new MyReferralResponse(
            x.ReferredUserId, x.ReferredName, x.ReferredPhoneNumber, x.RegisteredAt,
            x.HasPurchasedSubscription, x.SubscriptionActive, x.SubscriptionPlanType, x.SubscriptionEndAt))
            .ToArray();

        return ApiResult<IReadOnlyList<MyReferralResponse>>.SuccessResult(response);
    }
}
