using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.GuildBotStatus;

public class GuildBotStatusEntity
{
    public ulong GuildId { get; set; }
    public string? ConnectedVoiceChannelName { get; set; }
    public int ConnectedVoiceUserCount { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    
    public GuildDataEntity Guild { get; set; } = null!;
}