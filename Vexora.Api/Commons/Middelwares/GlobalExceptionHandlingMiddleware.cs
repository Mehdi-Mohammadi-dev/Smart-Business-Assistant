using FluentValidation;
using Vexora.Api.Commons.Results;
using Vexora.Api.Constants.Shared;
using Vexora.Application.Exceptions;
using Vexora.Domain.Exceptions.Authentications;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Domain.Exceptions.Sales.Items;
using Vexora.Domain.Exceptions.Subscriptions;
using Vexora.Domain.Exceptions.Users;
using Vexora.Domain.sharedKernel.Exceptions;

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
            // The client closed the connection. This is not an error.
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

        // Expected errors (400, 401, 409, ...) are only warnings. Only real server errors are "Error".
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

        // If the response already started, we can no longer change the status code or body.
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
            // FluentValidation
            ValidationException validationException
                => (StatusCodes.Status400BadRequest, CreateValidationResult(validationException)),

            // Business errors from the Application layer (OTP, login, SMS, ...)
            AppException appException
                => (
                    ToStatusCode(appException.Type),
                    ApiResult<object>.Failure(
                        appException.Message,
                        new ApiError(appException.Code, appException.Message))
                ),

            // Request validation
            ArgumentNullException or BadHttpRequestException
                => BadRequest("INVALID_REQUEST", "اطلاعات درخواست نامعتبر است."),

            // Entity not found
            EntityNotFoundException
                => (
                    StatusCodes.Status404NotFound,
                    ApiResult<object>.Failure(
                        Messages.NotFoundInformation,
                        new ApiError("ENTITY_NOT_FOUND", Messages.NotFoundInformation))
                ),

            // Shared domain exceptions
            SharedExceptions.InvalidIdException
                => BadRequest("INVALID_ID", exception.Message),

            SharedExceptions.AmountInvalidException
                => BadRequest("INVALID_AMOUNT", exception.Message),

            // User exceptions
            UserExceptions.PhoneNumberInvalidException
                => BadRequest("INVALID_PHONE_NUMBER", "شماره موبایل معتبر نیست."),

            UserExceptions.FirstNameInvalidException
                => BadRequest("INVALID_FIRST_NAME", exception.Message),

            UserExceptions.LastNameInvalidException
                => BadRequest("INVALID_LAST_NAME", exception.Message),

            UserExceptions.NotStrongPasswordException
                => BadRequest("WEAK_PASSWORD", exception.Message),

            AuthenticationExceptions.PhoneNumberRequired
                => BadRequest("PHONE_NUMBER_REQUIRED", "شماره موبایل الزامی است."),

            // Customer exceptions
            CustomerExceptions.FirstNameInvalidException
                => BadRequest("INVALID_CUSTOMER_FIRST_NAME", exception.Message),

            CustomerExceptions.LastNameInvalidException
                => BadRequest("INVALID_CUSTOMER_LAST_NAME", exception.Message),

            // Sale item exceptions
            SaleItemExceptions.QuantityException
                => BadRequest("INVALID_QUANTITY", exception.Message),

            SaleItemExceptions.UnitPriceException
                => BadRequest("INVALID_UNIT_PRICE", exception.Message),

            // Subscription exceptions
            SubscriptionExceptions.InvalidStartAndEndDate
                => BadRequest("INVALID_SUBSCRIPTION_DATES", exception.Message),

            // Unexpected exceptions: never show technical details to the user
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
        AppErrorType.NotFound => StatusCodes.Status404NotFound,
        AppErrorType.Conflict => StatusCodes.Status409Conflict,
        AppErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
        AppErrorType.ExternalService => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError
    };

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
