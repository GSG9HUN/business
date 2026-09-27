namespace DC_bot.Entities.MobileApps;

public class MobileRealtimeGuildMembershipEntity
{
    public string ConnectionId { get; set; } = string.Empty;
    public ulong DiscordUserId { get; set; }
    public ulong GuildId { get; set; }
    public DateTimeOffset JoinedAtUtc { get; set; }
}
