using DC_bot.Entities.Guilds;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Entities.Queue;

public class GuildQueueItemEntity
{
    public long Id { get; set; }
    public ulong GuildId { get; set; }
    public int Position { get; set; }
    public string TrackIdentifier { get; set; } = string.Empty;
    public QueueItemState State { get; set; }
    public DateTimeOffset AddedAtUtc { get; set; }
    public DateTimeOffset? PlayedAtUtc { get; set; }
    public DateTimeOffset? SkippedAtUtc { get; set; }

    public GuildDataEntity Guild { get; set; } = null!;
}
