namespace DC_bot.Persistence.Entities;

public class GuildDataEntity
{
	public long GuildId { get; set; }
	public bool IsPremium { get; set; }
	public DateTimeOffset? PremiumUntilUtc { get; set; }
	public DateTimeOffset UpdatedAtUtc { get; set; }

	public GuildPlaybackStateEntity? PlaybackState { get; set; }
	public ICollection<GuildQueueItemEntity> QueueItems { get; set; } = new List<GuildQueueItemEntity>();
	public ICollection<GuildRepeatListItemEntity> RepeatListItems { get; set; } = new List<GuildRepeatListItemEntity>();
	public ICollection<GuildPremiumAuditEntity> PremiumAuditEntries { get; set; } = new List<GuildPremiumAuditEntity>();
}