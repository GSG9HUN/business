using API.Responses.Guilds;
using API.Responses.Playback;
using API.Responses.Queue;

namespace API.Realtime.Snapshots.Interface;

public interface IRealtimeSnapshotProvider
{
    Task<PlaybackStatusResponse> GetPlaybackSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task<QueueResponse> GetQueueSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task<GuildBotStatusResponse> GetGuildBotStatusSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
}
