namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

/// <summary>
/// Carries immutable mobile app user guild upsert values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "Name">The stored guild-owned playlist name.</param>
/// <param name = "IconHash">The icon hash value used by this storage contract.</param>
/// <param name = "Permissions">The permissions value used by this storage contract.</param>
/// <param name = "IsOwner">The is owner value used by this storage contract.</param>
public sealed record MobileAppUserGuildUpsertRecord(ulong GuildId, string Name, string? IconHash, ulong Permissions, bool IsOwner);
