using DC_bot.Entities.Guilds;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Entities.Queue;

public class GuildQueueItemEntity
{
    public long Id { get; set; }
    public ulong GuildId { get; set; }
    public int Position { get; set; }
    public string TrackIdentifier { get; set; } = string.Empty;
    public string? SourceQuery { get; set; }
    public string? SourceSearchMode { get; set; }
    public string? RequestedBy { get; set; }
    public Guid? PlaybackOperationId { get; set; }
    public QueueItemState? StartPreviousState { get; set; }
    public Guid? StartPreviousOperationId { get; set; }
    public string? PlaybackNodeAddress { get; set; }
    public string? PlaybackNodeSessionId { get; set; }
    public bool PlaybackTransportConfirmed { get; set; }
    public QueueItemState State { get; set; }
    public DateTimeOffset AddedAtUtc { get; set; }
    public DateTimeOffset? PlayedAtUtc { get; set; }
    public DateTimeOffset? SkippedAtUtc { get; set; }

    public GuildDataEntity Guild { get; set; } = null!;
}
