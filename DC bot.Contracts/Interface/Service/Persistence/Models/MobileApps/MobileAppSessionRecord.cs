namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

/// <summary>
/// Carries immutable mobile app session values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "SessionId">The session id value used by this storage contract.</param>
/// <param name = "DiscordUserId">The discord user id value used by this storage contract.</param>
/// <param name = "RefreshTokenHash">The hashed refresh-token identity; plaintext token secrets are not stored here.</param>
/// <param name = "RefreshTokenExpiresAtUtc">The refresh token expires at utc value used by this storage contract.</param>
/// <param name = "RevokedAtUtc">The revoked at utc value used by this storage contract.</param>
public sealed record MobileAppSessionRecord(
    Guid SessionId,
    ulong DiscordUserId,
    string RefreshTokenHash,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc);
