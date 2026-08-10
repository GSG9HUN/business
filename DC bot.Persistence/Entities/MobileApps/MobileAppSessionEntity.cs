namespace DC_bot.Entities.MobileApps;

public class MobileAppSessionEntity
{
    public Guid SessionId { get; set; }
    public ulong DiscordUserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastRefreshedAtUtc { get; set; }
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    
    public MobileAppUserEntity User { get; set; } = null!;
}