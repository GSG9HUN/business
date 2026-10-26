using API.Snapshots.Playback.Interface;
using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Player;

/// <summary>
/// Returns an authorized guild playback snapshot using the shared snapshot service.
/// </summary>
public static class PlayerHandler
{

    /// <summary>
    /// Returns a guild&apos;s assembled playback read model with bot presence and queue metadata.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="playbackSnapshotService">The injected playback snapshot service dependency.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> GetSnapshotAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaybackSnapshotService playbackSnapshotService,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var(discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        var snapshot = await playbackSnapshotService.GetSnapshotAsync(guildId, discordUserId, cancellationToken);
        return HttpResults.Ok(snapshot);
    }
}
