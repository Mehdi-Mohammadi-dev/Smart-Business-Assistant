using FluentValidation;
using Vexora.Api.Commons.Results;
using Vexora.Api.Constants.Shared;
using Vexora.Application.Exceptions;
using Vexora.Application.Exceptions.SubscriptionPlans;
using Vexora.Domain.Exceptions.Authentications;
using Vexora.Domain.Exceptions.Checks;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Domain.Exceptions.Sales.Items;
using Vexora.Domain.Exceptions.Subscriptions;
using Vexora.Domain.Exceptions.Users;
using Vexora.Domain.sharedKernel.Exceptions;
using AppErrorType = Vexora.Domain.sharedKernel.Exceptions.AppErrorType;

namespace Vexora.Api.Commons.Middelwares;

public sealed class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled by the client. Path: {Path}", context.Request.Path);
            context.Response.StatusCode = 499;
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, result) = Map(exception);

        if (statusCode >= 500)
        {
            logger.LogError(exception,
                "Unhandled exception. Path: {Path}", context.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Handled {ExceptionType} ({StatusCode}) on {Path}: {Message}",
                exception.GetType().Name, statusCode, context.Request.Path, exception.Message);
        }

        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(result);
    }

    private static (int StatusCode, ApiResult<object> Result) Map(Exception exception)
    {

        return exception switch
        {
            ValidationException validationException
                => (StatusCodes.Status400BadRequest, CreateValidationResult(validationException)),

            // Specific exceptions must be checked before the base AppException.
            CheckNotPendingException
                => Conflict("CHECK_NOT_PENDING", exception.Message),

            CheckAlreadyPendingException
                => Conflict("CHECK_ALREADY_PENDING", exception.Message),

            InvalidStartAndEndDate
                => BadRequest("INVALID_SUBSCRIPTION_DATES", exception.Message),

            ReferralCodeInvalidException
                => BadRequest("INVALID_REFERRAL_CODE", exception.Message),

            ReferralProgramDisabledException
                => Conflict("REFERRAL_PROGRAM_DISABLED", exception.Message),

            // General application/domain exception must come after specific exceptions.
            Vexora.Domain.sharedKernel.Exceptions.AppException appException
                => (
                    ToStatusCode(appException.Type),
                    ApiResult<object>.Failure(
                        appException.Message,
                        new ApiError(appException.Code, appException.Message))
                ),

            ArgumentNullException or ArgumentOutOfRangeException or BadHttpRequestException
                => BadRequest("INVALID_REQUEST", "اطلاعات درخواست نامعتبر است."),

            EntityNotFoundException
                => (
                    StatusCodes.Status404NotFound,
                    ApiResult<object>.Failure(
                        Messages.NotFoundInformation,
                        new ApiError("ENTITY_NOT_FOUND", Messages.NotFoundInformation))
                ),

            InvalidIdException
                => BadRequest("INVALID_ID", exception.Message),

            AmountInvalidException
                => BadRequest("INVALID_AMOUNT", exception.Message),

            PhoneNumberInvalidException
                => BadRequest("INVALID_PHONE_NUMBER", "شماره موبایل معتبر نیست."),

            Vexora.Domain.Exceptions.Users.FirstNameInvalidException
                => BadRequest("INVALID_FIRST_NAME", exception.Message),

            Vexora.Domain.Exceptions.Users.LastNameInvalidException
                => BadRequest("INVALID_LAST_NAME", exception.Message),

            NotStrongPasswordException
                => BadRequest("WEAK_PASSWORD", exception.Message),

            PhoneNumberRequired
                => BadRequest("PHONE_NUMBER_REQUIRED", "شماره موبایل الزامی است."),

            Vexora.Domain.Exceptions.Customers.FirstNameInvalidException
                => BadRequest("INVALID_CUSTOMER_FIRST_NAME", exception.Message),

            Vexora.Domain.Exceptions.Customers.LastNameInvalidException
                => BadRequest("INVALID_CUSTOMER_LAST_NAME", exception.Message),

            QuantityException
                => BadRequest("INVALID_QUANTITY", exception.Message),

            UnitPriceException
                => BadRequest("INVALID_UNIT_PRICE", exception.Message),

            InvalidOperationException
                => Conflict("INVALID_OPERATION", exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                ApiResult<object>.Failure(
                    "خطایی در پردازش درخواست رخ داد.",
                    new ApiError("INTERNAL_SERVER_ERROR", "خطایی در سمت سرور رخ داده است."))
            )
        };
    }

    private static int ToStatusCode(AppErrorType type) => type switch
    {
        AppErrorType.Validation => StatusCodes.Status400BadRequest,
        AppErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        AppErrorType.PaymentRequired => StatusCodes.Status402PaymentRequired,
        AppErrorType.Forbidden => StatusCodes.Status403Forbidden,
        AppErrorType.NotFound => StatusCodes.Status404NotFound,
        AppErrorType.Conflict => StatusCodes.Status409Conflict,
        AppErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
        AppErrorType.ExternalService => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError
    };

    private static (int StatusCode, ApiResult<object> Result) Conflict(string code, string message)
    {
        return (
            StatusCodes.Status409Conflict,
            ApiResult<object>.Failure(message, new ApiError(code, message))
        );
    }

    private static (int StatusCode, ApiResult<object> Result) BadRequest(string code, string message)
    {
        return (
            StatusCodes.Status400BadRequest,
            ApiResult<object>.Failure(message, new ApiError(code, message))
        );
    }

    private static ApiResult<object> CreateValidationResult(ValidationException exception)
    {
        var errors = exception.Errors
            .Select(error => new ApiError("VALIDATION_ERROR", error.ErrorMessage, error.PropertyName))
            .ToArray();

        return ApiResult<object>.Failure(Messages.INvalidInformation, errors);
    }
}
