using API.Mapping;
using API.Responses.Playback;
using API.Snapshots.Playback.Interface;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Playback;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;

namespace API.Snapshots.Playback;

/// <summary>
/// Combines persisted playback, bot presence, and queue data into a REST and realtime read model.
/// </summary>
/// <param name="mobileAppUserRepository">The injected persistence contract used by this operation.</param>
/// <param name="guildBotStatusRepository">The injected persistence contract used by this operation.</param>
/// <param name="playbackStateRepository">The injected persistence contract used by this operation.</param>
/// <param name="queueRepository">The injected persistence contract used by this operation.</param>
public sealed class PlaybackSnapshotService(
    IMobileAppUserRepository mobileAppUserRepository,
    IGuildBotStatusRepository guildBotStatusRepository,
    IPlaybackStateRepository playbackStateRepository,
    IQueueRepository queueRepository) : IPlaybackSnapshotService
{

    /// <summary>
    /// Returns a guild&apos;s assembled playback read model with bot presence and queue metadata.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="userId">The user id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get snapshot result.</returns>
    public async Task<PlaybackStatusResponse> GetSnapshotAsync(
        ulong guildId,
        ulong? userId = null,
        CancellationToken cancellationToken = default)
    {
        var guild = userId is null ? null : await mobileAppUserRepository.GetGuildForUserAsync(userId.Value, guildId, cancellationToken);
        var botStatuses = await guildBotStatusRepository.GetByGuildIdsAsync([guildId], cancellationToken);
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
        var queuedItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        var currentQueueItem = state.QueueItemId is null ? null : await queueRepository.GetByIdAsync(state.QueueItemId.Value, cancellationToken);
        var currentTrack = state.CurrentTrackIdentifier is null ? null : TrackResponseMapper.TryMapPlaybackTrack(state.CurrentTrackIdentifier, currentQueueItem?.RequestedBy);
        var isPaused = currentTrack is not null && state.IsPaused;
        var positionSeconds = currentTrack is null ? 0 : CalculatePositionSeconds(state, currentTrack.Duration);
        return new PlaybackStatusResponse(guildId.ToString(), guild?.Name ?? string.Empty, guild is null ? null : GuildResponseMapper.MapGuildIconUrl(guild), GuildResponseMapper.MapBotStatus(botStatuses.GetValueOrDefault(guildId)), currentTrack, currentTrack is not null && !isPaused, isPaused, positionSeconds, queuedItems.Count, state.IsRepeating, state.IsRepeatingList, state.UpdatedAtUtc);
    }

    /// <summary>
    /// Advances an unpaused persisted position by elapsed time and clamps it to a known track duration.
    /// </summary>
    /// <param name="state">The state value for this operation.</param>
    /// <param name="durationSeconds">The duration seconds value for this operation.</param>
    /// <returns>The computed calculate position seconds result.</returns>
    private static int CalculatePositionSeconds(PlaybackStateRecord state, int durationSeconds)
    {
        var positionSeconds = Math.Max(0, state.PositionSeconds);

        if (state is { IsPaused: false, PositionUpdatedAtUtc: { } updatedAtUtc })
        {
            positionSeconds += TrackResponseMapper.ToDurationSeconds(DateTimeOffset.UtcNow - updatedAtUtc);
        }

        return durationSeconds > 0 ? Math.Min(positionSeconds, durationSeconds) : positionSeconds;
    }
}
