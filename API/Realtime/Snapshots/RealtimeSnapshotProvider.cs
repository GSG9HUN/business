using API.Mapping;
using API.Realtime.Snapshots.Interface;
using API.Responses.Guilds;
using API.Responses.Playback;
using API.Responses.Queue;
using API.Snapshots.Playback.Interface;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Queue;

namespace API.Realtime.Snapshots;

/// <summary>
/// Assembles realtime queue, playback, and bot presence snapshots from persistence contracts.
/// </summary>
/// <param name="playbackSnapshotService">The injected playback snapshot service dependency.</param>
/// <param name="queueRepository">The injected persistence contract used by this operation.</param>
/// <param name="guildBotStatusRepository">The injected persistence contract used by this operation.</param>
public sealed class RealtimeSnapshotProvider(
    IPlaybackSnapshotService playbackSnapshotService,
    IQueueRepository queueRepository,
    IGuildBotStatusRepository guildBotStatusRepository) : IRealtimeSnapshotProvider
{

    /// <summary>
    /// Reads the shared playback snapshot for a guild.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get playback snapshot result.</returns>
    public async Task<PlaybackStatusResponse> GetPlaybackSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        return await playbackSnapshotService.GetSnapshotAsync(guildId, null, cancellationToken);
    }

    /// <summary>
    /// Reads persisted queued items and maps them to a guild queue snapshot.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get queue snapshot result.</returns>
    public async Task<QueueResponse> GetQueueSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        var queueItems = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
        return QueueResponseMapper.Map(guildId, queueItems);
    }

    /// <summary>
    /// Reads persisted bot presence and maps missing status to disconnected defaults.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get guild bot status snapshot result.</returns>
    public async Task<GuildBotStatusResponse> GetGuildBotStatusSnapshotAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        var statuses = await guildBotStatusRepository.GetByGuildIdsAsync([guildId], cancellationToken);
        return GuildResponseMapper.MapBotStatus(statuses.GetValueOrDefault(guildId));
    }
}
