using API.Mapping;
using API.Responses.Playback;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Playback;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Player;

public static class PlayerHandler
{
    public static async Task<IResult> GetSnapshotAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaybackStateRepository playbackStateRepository,
        IQueueRepository queueRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
        var queuedItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        var currentTrack = state.CurrentTrackIdentifier is null
            ? null
            : TrackResponseMapper.TryMapPlaybackTrack(state.CurrentTrackIdentifier);
        var isPaused = currentTrack is not null && state.IsPaused;
        var positionSeconds = currentTrack is null
            ? 0
            : CalculatePositionSeconds(state, currentTrack.Duration);

        return HttpResults.Ok(new PlaybackStatusResponse(
            guildId.ToString(),
            currentTrack,
            currentTrack is not null && !isPaused,
            isPaused,
            positionSeconds,
            queuedItems.Count,
            state.IsRepeating,
            state.IsRepeatingList,
            state.UpdatedAtUtc));
    }

    private static int CalculatePositionSeconds(PlaybackStateRecord state, int durationSeconds)
    {
        var positionSeconds = Math.Max(0, state.PositionSeconds);

        if (!state.IsPaused && state.PositionUpdatedAtUtc is { } updatedAtUtc)
        {
            positionSeconds += TrackResponseMapper.ToDurationSeconds(DateTimeOffset.UtcNow - updatedAtUtc);
        }

        return durationSeconds > 0
            ? Math.Min(positionSeconds, durationSeconds)
            : positionSeconds;
    }
}
