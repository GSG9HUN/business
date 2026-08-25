using API.Errors;
using API.Mapping;
using API.Results;
using DC_bot.Interface.Service.Persistence.Status;

namespace API.Handlers.Status;

public static class StatusHandler
{
    public static async Task<IResult> ExecuteAsync(IDbStatusCheck statusCheckRepository)
    {
        ApiResult<object> result;
        try
        {
            var dbIsAlive = await statusCheckRepository.CanConnectAsync();
            if(dbIsAlive == false)
            {
                result = ApiResult<object>.Fail(ApiErrorCode.DbUnavailable, "Database connection failed.");
                return DomainToHttpMapper.ToHttpResult(result);
            }
            
            result = ApiResult<object>.Ok(200);
            return DomainToHttpMapper.ToHttpResult(result);

        }
        catch (Exception ex)
        {
            result = ApiResult<object>.Fail(ApiErrorCode.DbUnavailable, ex.Message);
            return DomainToHttpMapper.ToHttpResult(result);
        }
    }

}