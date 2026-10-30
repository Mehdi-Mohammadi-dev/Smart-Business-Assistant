using Microsoft.AspNetCore.Mvc.Filters;
using Vexora.Application.Exceptions;
using Vexora.Application.Repositories.Subscriptions;
using Vexora.Domain.Enums.UserRoles;

namespace Vexora.Api.Commons.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireActiveSubscriptionAttribute : Attribute, IAsyncActionFilter
{
    private const string CacheKey = "__active_subscription_checked";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.IsInRole(nameof(UserRole.SystemAdmin)))
        {
            await next();
            return;
        }

        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            await next();
            return;
        }

        if (context.HttpContext.Items.TryGetValue(CacheKey, out var cached) && cached is true)
        {
            await next();
            return;
        }

        var userIdClaim = context.HttpContext.User.FindFirst("sub")?.Value;
        if (!long.TryParse(userIdClaim, out var userId) || userId <= 0)
            throw new Vexora.Application.Exceptions.Authentications.UnauthorizedException();

        var repository = context.HttpContext.RequestServices.GetRequiredService<ISubscriptionRepository>();
        var timeProvider = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>();
        var subscription = await repository.GetLatestByUserIdAsync(userId, context.HttpContext.RequestAborted);
        if (subscription is null || !subscription.IsActiveAt(timeProvider.GetUtcNow()))
            throw new SubscriptionExpiredException();

        context.HttpContext.Items[CacheKey] = true;
        await next();
    }
}
