using API.Errors;
using API.Mapping;
using API.Results;
using DC_bot.Interface.Service.Persistence.Status;

namespace API.Handlers.Status;

/// <summary>
/// Reports database connectivity through the API result-to-HTTP mapping.
/// </summary>
public static class StatusHandler
{

    /// <summary>
    /// Checks database connectivity with caller cancellation and maps connectivity failures into HTTP results.
    /// </summary>
    /// <param name="statusCheckRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> ExecuteAsync(IDbStatusCheck statusCheckRepository, CancellationToken cancellationToken)
    {
        ApiResult<object> result;
        try
        {
            var dbIsAlive = await statusCheckRepository.CanConnectAsync(cancellationToken);

            if (dbIsAlive == false)
            {
                result = ApiResult<object>.Fail(ApiErrorCode.DbUnavailable, "Database connection failed.");
                return DomainToHttpMapper.ToHttpResult(result);
            }

            result = ApiResult<object>.Ok(200);
            return DomainToHttpMapper.ToHttpResult(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            result = ApiResult<object>.Fail(ApiErrorCode.DbUnavailable, ex.Message);
            return DomainToHttpMapper.ToHttpResult(result);
        }
    }
}
