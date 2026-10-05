using Vexora.Api.Constants.Shared;

namespace Vexora.Api.Commons.Results;

public sealed record ApiResult<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IReadOnlyList<ApiError> Errors { get; init; } = [];

    public static ApiResult<T> SuccessResult(
        T data,
        string message = Messages.Success)
    {
        return new ApiResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResult<T> Failure(
        string message,
        params ApiError[] errors)
    {
        return new ApiResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
