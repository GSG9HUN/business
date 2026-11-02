namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

/// <summary>
/// Carries immutable playlist values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "Name">The stored guild-owned playlist name.</param>
public sealed record PlaylistRecord(long Id, ulong GuildId, string Name);
