using API.Errors;
using API.Results;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Mapping;

/// <summary>
/// Translates API success and error results into HTTP status codes and response bodies.
/// </summary>
public static class DomainToHttpMapper
{

    /// <summary>
    /// Maps a success value or API error code to its corresponding HTTP result.
    /// </summary>
    /// <param name="result">The result value for this operation.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static IResult ToHttpResult<T>(ApiResult<T> result) => result.Success ? HttpResults.Ok(result.Value) : result.ErrorCode switch
    {
        ApiErrorCode.NotFound => HttpResults.NotFound(new { result.ErrorMessage }),
        ApiErrorCode.Conflict => HttpResults.Conflict(new { result.ErrorMessage }),
        ApiErrorCode.Validation => HttpResults.BadRequest(new { result.ErrorMessage }),
        ApiErrorCode.InvalidInput => HttpResults.BadRequest(new { result.ErrorMessage }),
        ApiErrorCode.Unauthorized => HttpResults.Unauthorized(),
        ApiErrorCode.NotConnected => HttpResults.Problem(result.ErrorMessage, statusCode: 503),
        ApiErrorCode.NoActivePlayer => HttpResults.Problem(result.ErrorMessage, statusCode: 409),
        ApiErrorCode.Forbidden => HttpResults.Problem(result.ErrorMessage, statusCode: 403),
        ApiErrorCode.DbUnavailable => HttpResults.Problem(result.ErrorMessage, statusCode: 503),
        _ => HttpResults.Problem(result.ErrorMessage ?? "Unknown error")};
}
