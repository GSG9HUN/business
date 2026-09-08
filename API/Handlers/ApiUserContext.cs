using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers;

internal static class ApiUserContext
{
    internal static bool TryGetDiscordUserId(HttpContext httpContext, out ulong discordUserId)
    {
        var userIdValue = httpContext.User.FindFirst("sub")?.Value;
        return ulong.TryParse(userIdValue, out discordUserId) && discordUserId != 0;
    }

    internal static async Task<(ulong DiscordUserId, IResult? Error)> RequireGuildAccessAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        ulong guildId,
        CancellationToken cancellationToken)
    {
        if (!TryGetDiscordUserId(httpContext, out var discordUserId))
        {
            return (0, HttpResults.Unauthorized());
        }

        var hasAccess = await userRepository.HasGuildAccessAsync(discordUserId, guildId, cancellationToken);
        return hasAccess
            ? (discordUserId, null)
            : (discordUserId, HttpResults.Forbid());
    }
}
