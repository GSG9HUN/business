namespace DC_bot.Persistence.Entities;

public class GuildPremiumAuditEntity
{
    public long Id { get; set; }
    public long GuildId { get; set; }
    public long? ChangedByUserId { get; set; }
    public bool OldIsPremium { get; set; }
    public bool NewIsPremium { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public string? Note { get; set; }

    public GuildDataEntity Guild { get; set; } = null!;
}
