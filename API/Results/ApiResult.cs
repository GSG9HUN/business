using API.Errors;

namespace API.Results;

/// <summary>
/// Carries either a typed API success value or an error code and message for HTTP mapping.
/// </summary>
/// <param name="Success">The success value for this operation.</param>
/// <param name="Value">The value value for this operation.</param>
/// <param name="ErrorCode">The error code value for this operation.</param>
/// <param name="ErrorMessage">The error message value for this operation.</param>
public sealed record ApiResult<T>(bool Success, T? Value, ApiErrorCode? ErrorCode, string? ErrorMessage)
{

    /// <summary>
    /// Creates a successful API result containing the supplied value.
    /// </summary>
    /// <param name="value">The value value for this operation.</param>
    /// <returns>The computed ok result.</returns>
    public static ApiResult<T> Ok(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failed API result containing an error code and message.
    /// </summary>
    /// <param name="code">The code value for this operation.</param>
    /// <param name="message">The message value for this operation.</param>
    /// <returns>The computed fail result.</returns>
    public static ApiResult<T> Fail(ApiErrorCode code, string message) => new(false, default, code, message);
}
