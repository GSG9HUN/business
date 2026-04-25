namespace DC_bot.Persistence.Entities;

public class GuildPlaybackStateEntity
{
	public long GuildId { get; set; }
	public bool IsRepeating { get; set; }
	public bool IsRepeatingList { get; set; }
	public string? CurrentTrackIdentifier { get; set; }
	public long? QueueItemId { get; set; }
	public DateTimeOffset UpdatedAtUtc { get; set; }

	public GuildDataEntity Guild { get; set; } = null!;
}