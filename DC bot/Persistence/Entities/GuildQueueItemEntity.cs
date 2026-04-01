namespace DC_bot.Persistence.Entities;

public class GuildQueueItemEntity
{
    public long Id { get; set; }
    public long GuildId { get; set; }
    public int Position { get; set; }
    public string TrackIdentifier { get; set; } = string.Empty;
    public short State { get; set; }
    public DateTimeOffset AddedAtUtc { get; set; }
    public DateTimeOffset? PlayedAtUtc { get; set; }
    public DateTimeOffset? SkippedAtUtc { get; set; }

    public GuildDataEntity Guild { get; set; } = null!;
}
