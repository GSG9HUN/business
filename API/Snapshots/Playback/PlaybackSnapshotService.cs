using API.Mapping;
using API.Responses.Playback;
using API.Snapshots.Playback.Interface;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Playback;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;

namespace API.Snapshots.Playback;

public sealed class PlaybackSnapshotService(
    IMobileAppUserRepository mobileAppUserRepository,
    IGuildBotStatusRepository guildBotStatusRepository,
    IPlaybackStateRepository playbackStateRepository,
    IQueueRepository queueRepository) : IPlaybackSnapshotService
{
    public async Task<PlaybackStatusResponse> GetSnapshotAsync(ulong guildId, ulong? userId = null,
        CancellationToken cancellationToken = default)
    {
        var guild = userId is null
            ? null
            : await mobileAppUserRepository.GetGuildForUserAsync(userId.Value, guildId, cancellationToken);
        var botStatuses = await guildBotStatusRepository.GetByGuildIdsAsync([guildId], cancellationToken);
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
        var queuedItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        var currentQueueItem = state.QueueItemId is null
            ? null
            : await queueRepository.GetByIdAsync(state.QueueItemId.Value, cancellationToken);
        var currentTrack = state.CurrentTrackIdentifier is null
            ? null
            : TrackResponseMapper.TryMapPlaybackTrack(state.CurrentTrackIdentifier, currentQueueItem?.RequestedBy);
        var isPaused = currentTrack is not null && state.IsPaused;
        var positionSeconds = currentTrack is null
            ? 0
            : CalculatePositionSeconds(state, currentTrack.Duration);

        return new PlaybackStatusResponse(
            guildId.ToString(),
            guild?.Name ?? string.Empty,
            guild is null ? null : GuildResponseMapper.MapGuildIconUrl(guild),
            GuildResponseMapper.MapBotStatus(botStatuses.GetValueOrDefault(guildId)),
            currentTrack,
            currentTrack is not null && !isPaused,
            isPaused,
            positionSeconds,
            queuedItems.Count,
            state.IsRepeating,
            state.IsRepeatingList,
            state.UpdatedAtUtc);
    }

    private static int CalculatePositionSeconds(PlaybackStateRecord state, int durationSeconds)
    {
        var positionSeconds = Math.Max(0, state.PositionSeconds);

        if (state is { IsPaused: false, PositionUpdatedAtUtc: { } updatedAtUtc })
        {
            positionSeconds += TrackResponseMapper.ToDurationSeconds(DateTimeOffset.UtcNow - updatedAtUtc);
        }

        return durationSeconds > 0
            ? Math.Min(positionSeconds, durationSeconds)
            : positionSeconds;
    }
}
