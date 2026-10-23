using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers;

/// <summary>
/// Resolves the authenticated Discord user and checks persisted guild access before handler operations.
/// </summary>
internal static class ApiUserContext
{

    /// <summary>
    /// Tries to read the authenticated Discord user identifier from the subject claim.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="discordUserId">Receives the discord user id result.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    internal static bool TryGetDiscordUserId(HttpContext httpContext, out ulong discordUserId)
    {
        var userIdValue = httpContext.User.FindFirst("sub")?.Value;
        return ulong.TryParse(userIdValue, out discordUserId) && discordUserId != 0;
    }

    /// <summary>
    /// Checks the subject claim and persisted membership, returning an HTTP access error when required.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
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
        return hasAccess ? (discordUserId, null) : (discordUserId, HttpResults.Forbid());
    }
}
