namespace Vexora.Api.Commons.Results;

public sealed record ApiError(
    string Code,
    string Message,
    string? Field = null);
