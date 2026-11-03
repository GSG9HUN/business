using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.MobileApps;

/// <summary>
/// Represents the persisted user guild row mapped by the EF Core model.
/// </summary>
public class UserGuildEntity
{

    /// <summary>
    /// The Discord user identifier.
    /// </summary>
    public ulong DiscordUserId { get; set; }

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The Discord permission bit field for the user's guild membership.
    /// </summary>
    public ulong Permissions { get; set; }

    /// <summary>
    /// Whether the user owns the guild.
    /// </summary>
    public bool IsOwner { get; set; }

    /// <summary>
    /// The display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The Discord guild icon hash, or null when no icon is set.
    /// </summary>
    public string? IconHash { get; set; }

    /// <summary>
    /// The UTC time at which this guild membership was last observed.
    /// </summary>
    public DateTimeOffset LastSeenAtUtc { get; set; }

    /// <summary>
    /// The user associated with this object.
    /// </summary>
    public MobileAppUserEntity User { get; set; } = null !;

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
