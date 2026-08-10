using API.Errors;
using API.Mapping;
using API.Results;
using DC_bot.Interface.Service.Persistence.MobileApps;

namespace API.Handlers.Guilds;

public static class GuildHandler
{
    public static async Task<IResult> Guilds(
        HttpContext httpContext,
        IMobileAppUserRepository repository, 
        CancellationToken cancellationToken)
    {
        var userIdValue = httpContext.User.FindFirst("sub")?.Value;
        ApiResult<object> result;
        
        if(!ulong.TryParse(userIdValue, out var discordUserId) || discordUserId == 0)
        {
            result = ApiResult<object>.Fail(ApiErrorCode.InvalidInput, "Invalid Discord user ID.");
            return DomainToHttpMapper.ToHttpResult(result);
        }
        
        var guilds = await repository.GetGuildsForUserAsync(discordUserId, cancellationToken);
        result = ApiResult<object>.Ok(guilds);
        return DomainToHttpMapper.ToHttpResult(result);
    }
}