using API.Errors;
using API.Results;
using HttpResults = Microsoft.AspNetCore.Http.Results;
namespace API.Mapping;

public static class DomainToHttpMapper
{
    public static IResult ToHttpResult<T>(ApiResult<T> result) =>
        result.Success
            ? HttpResults.Ok(result.Value)
            : result.ErrorCode switch
            {
                ApiErrorCode.NotFound => HttpResults.NotFound(new { result.ErrorMessage }),
                ApiErrorCode.Conflict => HttpResults.Conflict(new { result.ErrorMessage }),
                ApiErrorCode.Validation => HttpResults.BadRequest(new { result.ErrorMessage }),
                ApiErrorCode.NotConnected => HttpResults.Problem(result.ErrorMessage, statusCode: 503),
                ApiErrorCode.NoActivePlayer => HttpResults.Problem(result.ErrorMessage, statusCode: 409),
                ApiErrorCode.Forbidden => HttpResults.Problem(result.ErrorMessage, statusCode: 403),
                ApiErrorCode.DbUnavailable => HttpResults.Problem(result.ErrorMessage, statusCode: 503),
                _ => HttpResults.Problem(result.ErrorMessage ?? "Unknown error")
            };

    public static ApiResult<TDto> MapPlaylistResult<TDomain, TDto>(ApiResult<TDomain> result, Func<TDomain, TDto> mapFunc)
    {
        if (result.Success)
        {
            var dto = mapFunc(result.Value!);
            return ApiResult<TDto>.Ok(dto);
        }
        else
        {
            return ApiResult<TDto>.Fail(result.ErrorCode!.Value, result.ErrorMessage!);
        }
    }
}