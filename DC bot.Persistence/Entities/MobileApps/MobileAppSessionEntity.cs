namespace DC_bot.Entities.MobileApps;

/// <summary>
/// Represents the persisted mobile app session row mapped by the EF Core model.
/// </summary>
public class MobileAppSessionEntity
{

    /// <summary>
    /// The identifier of the mobile application session.
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// The Discord user identifier.
    /// </summary>
    public ulong DiscordUserId { get; set; }

    /// <summary>
    /// The hash of the refresh token; the raw token is not persisted here.
    /// </summary>
    public string RefreshTokenHash { get; set; } = string.Empty;

    /// <summary>
    /// The UTC time at which this record was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// The UTC time at which the session tokens were last refreshed.
    /// </summary>
    public DateTimeOffset LastRefreshedAtUtc { get; set; }

    /// <summary>
    /// The UTC expiration time of the refresh token.
    /// </summary>
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }

    /// <summary>
    /// The UTC revocation time, or null while the session has not been revoked.
    /// </summary>
    public DateTimeOffset? RevokedAtUtc { get; set; }

    /// <summary>
    /// The user associated with this object.
    /// </summary>
    public MobileAppUserEntity User { get; set; } = null !;
}
