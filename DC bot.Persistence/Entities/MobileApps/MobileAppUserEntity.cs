namespace DC_bot.Entities.MobileApps;

public class MobileAppUserEntity
{
    public ulong DiscordUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? GlobalName { get; set; } = string.Empty;
    public string? AvatarHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastLoginAtUtc { get; set; }
    
    public ICollection<UserGuildEntity> Guilds { get; set; } = new List<UserGuildEntity>();
}