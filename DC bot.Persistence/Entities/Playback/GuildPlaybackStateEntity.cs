using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.Playback;

public class GuildPlaybackStateEntity
{
	public ulong GuildId { get; set; }
	public bool IsRepeating { get; set; }
	public bool IsRepeatingList { get; set; }
	public bool IsPaused { get; set; }
	public int PositionSeconds { get; set; }
	public DateTimeOffset? PositionUpdatedAtUtc { get; set; }
	public string? CurrentTrackIdentifier { get; set; }
	public long? QueueItemId { get; set; }
	public DateTimeOffset UpdatedAtUtc { get; set; }

	public GuildDataEntity Guild { get; set; } = null!;
}
