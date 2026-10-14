using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vexora.Api.Commons.Results;
using Vexora.Api.Constants.Shared;

namespace Vexora.Api.Commons.Filters;

public sealed class ValidationFilter(
    IServiceProvider serviceProvider)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Broken JSON / wrong data types. (The automatic 400 is switched off in Program.cs,
        // so we must handle this ourselves, otherwise the action receives null.)
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(
                ApiResult<object>.Failure(
                    Messages.INvalidInformation,
                    new ApiError("INVALID_REQUEST", Messages.INvalidInformation)));
            return;
        }

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
