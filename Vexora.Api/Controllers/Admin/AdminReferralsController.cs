using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Admin.Referrals;
using Vexora.Api.Mappers.Admin;
using Vexora.Application.Services.Interfaces.SubscriptionPlans;
using Vexora.Domain.Enums.UserRoles;

namespace Vexora.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/referrals")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public sealed class AdminReferralsController(
    IReferralService referralService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("گزارش کدهای معرف") ]
    [EndpointDescription("نمایش می‌دهد هر کاربر توسط چه کسی معرفی شده و معرف چند ثبت‌نام موفق داشته است.")]
    public async Task<ApiResult<IReadOnlyList<ReferralResponse>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new BadHttpRequestException("پارامترهای صفحه‌بندی نامعتبر است.");

        var rows = await referralService.ListAsync(page, pageSize, cancellationToken);
        return ApiResult<IReadOnlyList<ReferralResponse>>.SuccessResult(rows.Select(x => x.ToContract()).ToArray());
    }

    [HttpGet("settings")]
    [EndpointSummary("مشاهده تنظیمات طرح معرفی")]
    public async Task<ApiResult<ReferralSettingResponse>> GetSettings(CancellationToken cancellationToken)
    {
        var dto = await referralService.GetSettingsAsync(cancellationToken);
        return ApiResult<ReferralSettingResponse>.SuccessResult(new ReferralSettingResponse(dto.IsEnabled, dto.RewardDays, dto.MaxRewardDaysPerUser));
    }

    [HttpPut("settings")]
    [EndpointSummary("تنظیم طرح معرفی")]
    public async Task<ApiResult<ReferralSettingResponse>> UpdateSettings(
        [FromBody] UpdateReferralSettingContract request, CancellationToken cancellationToken)
    {
        var dto = await referralService.UpdateSettingsAsync(
            new Vexora.Application.Dtos.SubscriptionPlans.ReferralSettingDto(request.IsEnabled, request.RewardDays, request.MaxRewardDaysPerUser), cancellationToken);
        return ApiResult<ReferralSettingResponse>.SuccessResult(new ReferralSettingResponse(dto.IsEnabled, dto.RewardDays, dto.MaxRewardDaysPerUser));
    }
}
