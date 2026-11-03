using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.GuildBotStatus;

/// <summary>
/// Represents the persisted guild bot status row mapped by the EF Core model.
/// </summary>
public class GuildBotStatusEntity
{

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The connected voice channel name, or null when the bot is disconnected.
    /// </summary>
    public string? ConnectedVoiceChannelName { get; set; }

    /// <summary>
    /// The persisted number of users in the bot's voice channel.
    /// </summary>
    public int ConnectedVoiceUserCount { get; set; }

    /// <summary>
    /// The UTC time of the most recent persisted update.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
