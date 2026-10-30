using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Application.Repositories.Subscriptions;
using Vexora.Application.Repositories.Users;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Domain.Enums.Subscriptions;
using Vexora.Domain.Enums.UserRoles;

namespace Vexora.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public sealed class AdminUsersController(
    IUserRepository users,
    ISubscriptionRepository subscriptions,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<AdminUsersController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("فهرست کاربران")]
    [EndpointDescription("کاربران را با امکان جست‌وجو و صفحه‌بندی نمایش می‌دهد.")]
    public async Task<ApiResult<IReadOnlyList<object>>> List(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new BadHttpRequestException("پارامترهای صفحه‌بندی نامعتبر است.");
        var results = await users.SearchAsync(search, page, pageSize, cancellationToken);
        var items = new List<object>();
        foreach (var user in results)
        {
            var sub = await subscriptions.GetLatestByUserIdAsync(user.Id, cancellationToken);
            var active = sub?.IsActiveAt(DateTimeOffset.UtcNow) == true;
            items.Add(new { user.Id, user.FirstName, user.LastName, user.PhoneNumber, RegisteredAt = user.CreateAt, SubscriptionActive = active, SubscriptionEndAt = sub?.EndAt });
        }
        return ApiResult<IReadOnlyList<object>>.SuccessResult(items);
    }

    [HttpPost("{id:long}/subscription")]
    [EndpointSummary("تمدید اشتراک کاربر")]
    [EndpointDescription("اشتراک کاربر را برای تعداد روز مشخص تمدید یا فعال می‌کند.")]
    public async Task<ApiResult<object>> Extend(long id, [FromBody] ExtendSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request.Days is < 1 or > 365 || !Enum.IsDefined(request.PlanType))
            throw new BadHttpRequestException("days باید بین ۱ و ۳۶۵ باشد و planType معتبر باشد.");
        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null)
            throw new Vexora.Application.Exceptions.EntityNotFoundException("User", id);
        var now = DateTimeOffset.UtcNow;
        var subscription = await subscriptions.GetLatestByUserIdAsync(id, cancellationToken);
        if (subscription is not null && subscription.IsActiveAt(now))
            subscription.Extend(request.Days);
        else
        {
            subscription = Subscription.Create(id, request.PlanType, now, now.AddDays(request.Days));
            subscription.Activate();
            await subscriptions.AddAsync(subscription, cancellationToken);
        }
        await unitOfWork.SaveAsync(cancellationToken);
        logger.LogInformation("Admin {AdminId} extended subscription for user {TargetUserId} by {Days} days", currentUser.UserId, id, request.Days);
        return ApiResult<object>.SuccessResult(new { subscription.StartAt, subscription.EndAt, PlanType = subscription.PlanType.ToString(), Status = subscription.Status.ToString() });
    }

    [HttpPost("{id:long}/subscription/cancel")]
    [EndpointSummary("لغو اشتراک کاربر")]
    [EndpointDescription("اشتراک فعلی کاربر را لغو می‌کند.")]
    public async Task<ApiResult<object?>> Cancel(long id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null)
            throw new Vexora.Application.Exceptions.EntityNotFoundException("User", id);
        var subscription = await subscriptions.GetLatestByUserIdAsync(id, cancellationToken);
        if (subscription is null)
            throw new Vexora.Application.Exceptions.EntityNotFoundException("Subscription", id);
        subscription.Cancel();
        subscription.SoftDelete();
        await unitOfWork.SaveAsync(cancellationToken);
        logger.LogInformation("Admin {AdminId} cancelled subscription for user {TargetUserId}", currentUser.UserId, id);
        return ApiResult<object?>.SuccessResult(null);
    }
}

public sealed record ExtendSubscriptionRequest(int Days, SubscriptionPlanType PlanType);
