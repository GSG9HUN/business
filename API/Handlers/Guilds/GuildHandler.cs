using API.Errors;
using API.Mapping;
using API.Responses.Guilds;
using API.Results;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace API.Handlers.Guilds;

public static class GuildHandler
{
    private const ulong AdministratorPermission = 1UL << 3;

    public static async Task<IResult> Guilds(
        HttpContext httpContext,
        IMobileAppUserRepository repository, 
        CancellationToken cancellationToken)
    {
        ApiResult<object> result;
        
        if(!ApiUserContext.TryGetDiscordUserId(httpContext, out var discordUserId))
        {
            result = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized Discord user.");
            return DomainToHttpMapper.ToHttpResult(result);
        }
        
        var guilds = await repository.GetGuildsForUserAsync(discordUserId, cancellationToken);
        var response = guilds.Select(MapGuild).ToList();

        result = ApiResult<object>.Ok(response);
        return DomainToHttpMapper.ToHttpResult(result);
    }

    private static GuildSummaryResponse MapGuild(MobileAppUserGuildRecord guild) =>
        new(
            guild.GuildId,
            guild.Name,
            BuildGuildIconUrl(guild.GuildId, guild.IconHash),
            GetAccessLevel(guild),
            new GuildBotStatusResponse(false, null, 0));

    private static string GetAccessLevel(MobileAppUserGuildRecord guild) =>
        guild.IsOwner || (guild.Permissions & AdministratorPermission) != 0
            ? "Admin"
            : "Member";

    private static string? BuildGuildIconUrl(ulong guildId, string? iconHash)
    {
        if (string.IsNullOrWhiteSpace(iconHash))
        {
            return null;
        }

        var extension = iconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "webp";
        return $"https://cdn.discordapp.com/icons/{guildId}/{iconHash}.{extension}?size=128";
    }
}
