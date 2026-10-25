using Microsoft.AspNetCore.Mvc.Filters;

namespace Vexora.Api.Commons.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireActiveSubscriptionAttribute : Attribute, IAsyncActionFilter
{
    public Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        return next();
    }
}
