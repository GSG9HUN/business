using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.Playback;

public class GuildRepeatListItemEntity
{
	public long Id { get; set; }
	public ulong GuildId { get; set; }
	public int Position { get; set; }
	public string TrackIdentifier { get; set; } = string.Empty;
	public DateTimeOffset AddedAtUtc { get; set; }

	public GuildDataEntity Guild { get; set; } = null!;

}