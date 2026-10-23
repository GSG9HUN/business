namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

/// <summary>
/// Carries immutable mobile app user upsert values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "DiscordUserId">The discord user id value used by this storage contract.</param>
/// <param name = "Username">The username value used by this storage contract.</param>
/// <param name = "GlobalName">The global name value used by this storage contract.</param>
/// <param name = "AvatarHash">The avatar hash value used by this storage contract.</param>
public sealed record MobileAppUserUpsertRecord(ulong DiscordUserId, string Username, string? GlobalName, string? AvatarHash);
