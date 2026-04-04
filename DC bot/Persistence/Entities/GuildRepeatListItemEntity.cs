namespace DC_bot.Persistence.Entities;

public class GuildRepeatListItemEntity
{
	public long Id { get; set; }
	public long GuildId { get; set; }
	public int Position { get; set; }
	public string TrackIdentifier { get; set; } = string.Empty;
	public DateTimeOffset AddedAtUtc { get; set; }

	public GuildDataEntity Guild { get; set; } = null!;

}