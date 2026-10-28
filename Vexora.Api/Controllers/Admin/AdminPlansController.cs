using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Application.Dtos.SubscriptionPlans;
using Vexora.Application.Services.Interfaces.SubscriptionPlans;
using Vexora.Domain.Enums.UserRoles;

namespace Vexora.Api.Controllers.Admin;

public sealed record PlanRequest(
    [Required, StringLength(100)] string Name,
    int Days, long PriceToman, int DiscountPercent = 0,
    bool IsActive = true, bool IsFeatured = false, int SortOrder = 0)
{
    public SaveSubscriptionPlanDto ToDto() =>
        new(Name, Days, PriceToman, DiscountPercent, IsActive, IsFeatured, SortOrder);
}

[ApiController]
[Route("api/admin/plans")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public sealed class AdminPlansController(ISubscriptionPlanService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("فهرست همه پلن‌ها (ادمین)")]
    public async Task<ApiResult<IReadOnlyList<SubscriptionPlanDto>>> List(CancellationToken ct)
        => ApiResult<IReadOnlyList<SubscriptionPlanDto>>.SuccessResult(await service.GetAllAsync(ct));

    [HttpPost]
    [EndpointSummary("ایجاد پلن")]
    [EndpointDescription("مبالغ به تومان. مقدار نامعتبر: 400 با کد INVALID_PLAN.")]
    public async Task<ApiResult<SubscriptionPlanDto>> Create([FromBody] PlanRequest r, CancellationToken ct)
        => ApiResult<SubscriptionPlanDto>.SuccessResult(await service.CreateAsync(r.ToDto(), ct));

    [HttpPut("{id:long}")]
    [EndpointSummary("ویرایش پلن")]
    public async Task<ApiResult<SubscriptionPlanDto>> Update(long id, [FromBody] PlanRequest r, CancellationToken ct)
        => ApiResult<SubscriptionPlanDto>.SuccessResult(await service.UpdateAsync(id, r.ToDto(), ct));

    [HttpDelete("{id:long}")]
    [EndpointSummary("غیرفعال‌سازی پلن")]
    public async Task<ApiResult<object?>> Delete(long id, CancellationToken ct)
    {
        await service.DeactivateAsync(id, ct);
        return ApiResult<object?>.SuccessResult(null);
    }
}

[ApiController]
[Route("api/plans")]
public sealed class PlansController(ISubscriptionPlanService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("فهرست پلن‌های فعال")]
    public async Task<ApiResult<IReadOnlyList<SubscriptionPlanDto>>> List(CancellationToken ct)
        => ApiResult<IReadOnlyList<SubscriptionPlanDto>>.SuccessResult(await service.GetActiveAsync(ct));
}
