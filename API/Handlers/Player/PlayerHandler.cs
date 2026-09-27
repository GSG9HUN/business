using API.Snapshots.Playback.Interface;
using DC_bot.Interface.Service.Persistence.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Player;

public static class PlayerHandler
{
    public static async Task<IResult> GetSnapshotAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaybackSnapshotService playbackSnapshotService,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var snapshot = await playbackSnapshotService.GetSnapshotAsync(guildId, discordUserId, cancellationToken);

        return HttpResults.Ok(snapshot);
    }
}