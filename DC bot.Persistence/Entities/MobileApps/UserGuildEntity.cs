using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.MobileApps;

public class UserGuildEntity
{
    public ulong DiscordUserId { get; set; }
    public ulong GuildId { get; set; }
    public ulong Permissions { get; set; }
    public bool IsOwner { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconHash { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
    
    public MobileAppUserEntity User { get; set; } = null!;
    public GuildDataEntity Guild { get; set; } = null!;
}