namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

/// <summary>
/// Carries immutable mobile app user values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "DiscordUserId">The discord user id value used by this storage contract.</param>
/// <param name = "Username">The username value used by this storage contract.</param>
/// <param name = "GlobalName">The global name value used by this storage contract.</param>
/// <param name = "AvatarHash">The avatar hash value used by this storage contract.</param>
/// <param name = "CreatedAtUtc">The created at utc value used by this storage contract.</param>
/// <param name = "LastLoginAtUtc">The last login at utc value used by this storage contract.</param>
public sealed record MobileAppUserRecord(
    ulong DiscordUserId,
    string Username,
    string? GlobalName,
    string? AvatarHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastLoginAtUtc);
