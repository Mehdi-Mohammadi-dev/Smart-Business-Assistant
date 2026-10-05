using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vexora.Api.Commons.Filters;

public sealed class ValidationFilter(
    IServiceProvider serviceProvider)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType =
                typeof(IValidator<>)
                    .MakeGenericType(argument.GetType());

            var validator =
                serviceProvider.GetService(validatorType);

            if (validator is null)
                continue;

            var validationContext =
                new ValidationContext<object>(argument);

            var result =
                await ((IValidator)validator)
                    .ValidateAsync(
                        validationContext,
                        context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                throw new ValidationException(
                    result.Errors);
            }
        }

        await next();
    }
}
