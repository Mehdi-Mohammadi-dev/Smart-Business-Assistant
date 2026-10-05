using FluentValidation;
using Vexora.Api.Commons.Results;
using Vexora.Api.Constants.Shared;
using Vexora.Application.Exceptions;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Domain.Exceptions.Sales.Items;
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        logger.LogError(
            exception,
            "Unhandled exception occurred. Path: {Path}",
            context.Request.Path);

        var (statusCode, result) = exception switch
        {
            // FluentValidation
            ValidationException validationException
                => (
                    StatusCodes.Status400BadRequest,
                    CreateValidationResult(validationException)
                ),

            // Request validation
            ArgumentNullException
                => (
                    StatusCodes.Status400BadRequest,
                    ApiResult<object>.Failure(
                        "درخواست نامعتبر است.",
                        new ApiError(
                            "INVALID_REQUEST",
                            "اطلاعات درخواست نامعتبر است."))
                ),

            // Entity not found
            EntityNotFoundException
                => (
                    StatusCodes.Status404NotFound,
                    ApiResult<object>.Failure(
                        Messages.NotFoundInformation,
                        new ApiError(
                            "ENTITY_NOT_FOUND",
                            Messages.NotFoundInformation))
                ),

            // Shared domain exceptions
            SharedExceptions.InvalidIdException
                => BadRequest(
                    "INVALID_ID",
                    exception.Message),

            SharedExceptions.AmountInvalidException
                => BadRequest(
                    "INVALID_AMOUNT",
                    exception.Message),

            // User exceptions
            UserExceptions.PhoneNumberInvalidException
                => BadRequest(
                    "INVALID_PHONE_NUMBER",
                    exception.Message),

            UserExceptions.FirstNameInvalidException
                => BadRequest(
                    "INVALID_FIRST_NAME",
                    exception.Message),

            UserExceptions.LastNameInvalidException
                => BadRequest(
                    "INVALID_LAST_NAME",
                    exception.Message),

            UserExceptions.NotStrongPasswordException
                => BadRequest(
                    "WEAK_PASSWORD",
                    exception.Message),

            // Customer exceptions
            CustomerExceptions.FirstNameInvalidException
                => BadRequest(
                    "INVALID_CUSTOMER_FIRST_NAME",
                    exception.Message),

            CustomerExceptions.LastNameInvalidException
                => BadRequest(
                    "INVALID_CUSTOMER_LAST_NAME",
                    exception.Message),

            // Sale item exceptions
            SaleItemExceptions.QuantityException
                => BadRequest(
                    "INVALID_QUANTITY",
                    exception.Message),

            SaleItemExceptions.UnitPriceException
                => BadRequest(
                    "INVALID_UNIT_PRICE",
                    exception.Message),

            // Unexpected exceptions
            _
                => (
                    StatusCodes.Status500InternalServerError,
                    ApiResult<object>.Failure(
                        "خطایی در پردازش درخواست رخ داد.",
                        new ApiError(
                            "INTERNAL_SERVER_ERROR",
                            "خطایی در سمت سرور رخ داده است."))
                )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(result);
    }

    private static (
        int StatusCode,
        ApiResult<object> Result)
        BadRequest(
            string code,
            string message)
    {
        return (
            StatusCodes.Status400BadRequest,
            ApiResult<object>.Failure(
                message,
                new ApiError(
                    code,
                    message))
        );
    }

    private static ApiResult<object> CreateValidationResult(
        ValidationException exception)
    {
        var errors = exception.Errors
            .Select(error =>
                new ApiError(
                    "VALIDATION_ERROR",
                    error.ErrorMessage,
                    error.PropertyName))
            .ToArray();

        return ApiResult<object>.Failure(
            Messages.INvalidInformation,
            errors);
    }
}
