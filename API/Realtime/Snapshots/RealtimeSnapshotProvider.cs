using API.Mapping;
using API.Realtime.Snapshots.Interface;
using API.Responses.Guilds;
using API.Responses.Playback;
using API.Responses.Queue;
using API.Snapshots.Playback.Interface;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Queue;

namespace API.Realtime.Snapshots;

public sealed class RealtimeSnapshotProvider(
    IPlaybackSnapshotService playbackSnapshotService,
    IQueueRepository queueRepository,
    IGuildBotStatusRepository guildBotStatusRepository
) : IRealtimeSnapshotProvider
{
    public async Task<PlaybackStatusResponse> GetPlaybackSnapshotAsync(ulong guildId,
        CancellationToken cancellationToken = default)
    {
        return await playbackSnapshotService.GetSnapshotAsync(guildId, null, cancellationToken);
    }

    public async Task<QueueResponse> GetQueueSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        var queueItems = await queueRepository.GetQueuedItemsAsync(
            guildId,
            cancellationToken);
        
        return QueueResponseMapper.Map(guildId, queueItems);
    }

    public async Task<GuildBotStatusResponse> GetGuildBotStatusSnapshotAsync(ulong guildId,
        CancellationToken cancellationToken = default)
    {
        var statuses = await guildBotStatusRepository.GetByGuildIdsAsync([guildId], cancellationToken);

        return GuildResponseMapper.MapBotStatus(statuses.GetValueOrDefault(guildId));
    }
}
