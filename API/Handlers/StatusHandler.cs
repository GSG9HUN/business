using API.Errors;
using API.Mapping;
using API.Results;
using DC_bot.Interface.Service.Persistence;
using static Microsoft.AspNetCore.Http.Results;

namespace API.Handlers;

public static class StatusHandler
{
    public static async Task<IResult> ExecuteAsync(IDbStatusCheck statusCheckRepository)
    {
        ApiResult<object> result;
        try
        {
            await statusCheckRepository.CanConnectAsync();
            result = ApiResult<object>.Ok(200);

            return DomainToHttpMapper.ToHttpResult(result);

        }
        catch (Exception ex)
        {
            result = ApiResult<object>.Fail(ApiErrorCode.DbUnavailable, "Database connection failed.");
            return DomainToHttpMapper.ToHttpResult(result);
        }
    }

}