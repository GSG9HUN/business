using API.Errors;

namespace API.Results;

public sealed record ApiResult<T>(bool Success, T? Value, ApiErrorCode? ErrorCode, string? ErrorMessage)
{
    public static ApiResult<T> Ok(T value) => new(true, value, null, null);
    public static ApiResult<T> Fail(ApiErrorCode code, string message) => new(false, default, code, message);
}