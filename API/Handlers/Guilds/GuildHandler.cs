using API.Errors;
using API.Mapping;
using API.Results;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Guilds;

public static class GuildHandler
{
    public static async Task<IResult> Guilds(
        HttpContext httpContext,
        IMobileAppUserRepository repository,
        IGuildBotStatusRepository botStatusRepository,
        CancellationToken cancellationToken)
    {
        ApiResult<object> result;
        
        if(!ApiUserContext.TryGetDiscordUserId(httpContext, out var discordUserId))
        {
            result = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized Discord user.");
            return DomainToHttpMapper.ToHttpResult(result);
        }
        
        var guilds = await repository.GetGuildsForUserAsync(discordUserId, cancellationToken);
        var botStatuses = await botStatusRepository.GetByGuildIdsAsync(
            guilds.Select(guild => guild.GuildId).ToArray(),
            cancellationToken);
        var response = guilds
            .Select(guild => GuildResponseMapper.MapGuild(
                guild,
                botStatuses.GetValueOrDefault(guild.GuildId)))
            .ToList();

        result = ApiResult<object>.Ok(response);
        return DomainToHttpMapper.ToHttpResult(result);
    }

    public static async Task<IResult> Guild(
        HttpContext httpContext,
        IMobileAppUserRepository repository,
        IGuildBotStatusRepository botStatusRepository,
        CancellationToken cancellationToken)
    {
        if(!ApiUserContext.TryGetDiscordUserId(httpContext, out var discordUserId))
        {
            return DomainToHttpMapper.ToHttpResult(
                ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized Discord user."));
        }

        var guildId = (ulong)httpContext.Items["guildId"]!;
        var guild = await repository.GetGuildForUserAsync(discordUserId, guildId, cancellationToken);
        if (guild is null)
        {
            return HttpResults.NotFound(new { ErrorMessage = "Guild not found." });
        }

        var botStatuses = await botStatusRepository.GetByGuildIdsAsync([guildId], cancellationToken);
        return HttpResults.Ok(GuildResponseMapper.MapGuild(
            guild,
            botStatuses.GetValueOrDefault(guildId)));
    }
}
