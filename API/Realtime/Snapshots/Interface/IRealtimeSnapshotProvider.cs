using API.Responses.Guilds;
using API.Responses.Playback;
using API.Responses.Queue;

namespace API.Realtime.Snapshots.Interface;

/// <summary>
/// Defines snapshot reads shared by realtime publishers.
/// </summary>
public interface IRealtimeSnapshotProvider
{

    /// <summary>
    /// Reads the shared playback snapshot for a guild.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get playback snapshot result.</returns>
    Task<PlaybackStatusResponse> GetPlaybackSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads persisted queued items and maps them to a guild queue snapshot.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get queue snapshot result.</returns>
    Task<QueueResponse> GetQueueSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads persisted bot presence and maps missing status to disconnected defaults.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get guild bot status snapshot result.</returns>
    Task<GuildBotStatusResponse> GetGuildBotStatusSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
}
