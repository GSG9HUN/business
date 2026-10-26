using DC_bot.Entities.GuildBotStatus;
using DC_bot.Entities.MobileApps;
using DC_bot.Entities.Playback;
using DC_bot.Entities.Playlists;
using DC_bot.Entities.Queue;

namespace DC_bot.Entities.Guilds;

/// <summary>
/// Represents the persisted guild data row mapped by the EF Core model.
/// </summary>
public class GuildDataEntity
{

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// Whether premium access is enabled for the guild.
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// The premium expiration time in UTC, or null when no expiration is recorded.
    /// </summary>
    public DateTimeOffset? PremiumUntilUtc { get; set; }

    /// <summary>
    /// The UTC time of the most recent persisted update.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
    
    /// <summary>
    /// Whether the bot should maintain a voice connection even without playback or other users.
    /// </summary>
    public bool IsTwentyFourSevenEnabled { get; set; }

    /// <summary>
    /// The voice channel to reconnect to while twenty-four-seven mode is enabled.
    /// </summary>
    public ulong? PreferredVoiceChannelId { get; set; }

    /// <summary>
    /// The text channel used for playback notifications associated with the persistent voice connection.
    /// </summary>
    public ulong? PreferredTextChannelId { get; set; }

    /// <summary>
    /// The guild's persisted playback state.
    /// </summary>
    public GuildPlaybackStateEntity? PlaybackState { get; set; }

    /// <summary>
    /// The guild's persisted voice connection status.
    /// </summary>
    public GuildBotStatusEntity? BotStatus { get; set; }

    /// <summary>
    /// The queue entries associated with the guild.
    /// </summary>
    public ICollection<GuildQueueItemEntity> QueueItems { get; set; } = new List<GuildQueueItemEntity>();

    /// <summary>
    /// The guild's persisted repeat-list snapshot entries.
    /// </summary>
    public ICollection<GuildRepeatListItemEntity> RepeatListItems { get; set; } = new List<GuildRepeatListItemEntity>();

    /// <summary>
    /// The audit entries recording changes to the guild's premium status.
    /// </summary>
    public ICollection<GuildPremiumAuditEntity> PremiumAuditEntries { get; set; } = new List<GuildPremiumAuditEntity>();

    /// <summary>
    /// The saved playlists associated with the guild.
    /// </summary>
    public ICollection<PlaylistEntity> Playlists { get; set; } = new List<PlaylistEntity>();

    /// <summary>
    /// The persisted user memberships associated with the guild.
    /// </summary>
    public ICollection<UserGuildEntity> UserGuilds { get; set; } = new List<UserGuildEntity>();
}
