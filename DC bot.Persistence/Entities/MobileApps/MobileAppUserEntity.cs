using DC_bot.Entities.MobileAppUserSettings;

namespace DC_bot.Entities.MobileApps;

/// <summary>
/// Represents the persisted mobile app user row mapped by the EF Core model.
/// </summary>
public class MobileAppUserEntity
{

    /// <summary>
    /// The Discord user identifier.
    /// </summary>
    public ulong DiscordUserId { get; set; }

    /// <summary>
    /// The Discord username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The Discord global display name, or null when it is not set.
    /// </summary>
    public string? GlobalName { get; set; } = string.Empty;

    /// <summary>
    /// The Discord avatar hash, or null when no custom avatar is set.
    /// </summary>
    public string? AvatarHash { get; set; } = string.Empty;

    /// <summary>
    /// The UTC time at which this record was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// The UTC time of the user's most recent application login.
    /// </summary>
    public DateTimeOffset LastLoginAtUtc { get; set; }

    /// <summary>
    /// The persisted guild memberships of the user.
    /// </summary>
    public ICollection<UserGuildEntity> Guilds { get; set; } = new List<UserGuildEntity>();

    /// <summary>
    /// The user's persisted application preferences.
    /// </summary>
    public MobileAppUserSettingsEntity? Settings { get; set; }
}
