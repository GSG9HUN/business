namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

/// <summary>
/// Carries immutable playlist summary values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "Name">The stored guild-owned playlist name.</param>
/// <param name = "TrackCount">The track count value used by this storage contract.</param>
public sealed record PlaylistSummaryRecord(long Id, ulong GuildId, string Name, int TrackCount);
