namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

/// <summary>Describes saved playlist metadata with its track count and optional custom cover.</summary>
/// <param name="Id">The playlist identifier.</param>
/// <param name="GuildId">The owning guild identifier.</param>
/// <param name="Name">The playlist name.</param>
/// <param name="TrackCount">The saved track count.</param>
/// <param name="ArtworkUri">The custom cover URL, or null for automatic artwork.</param>
public sealed record PlaylistSummaryRecord(
    long Id,
    ulong GuildId,
    string Name,
    int TrackCount,
    string? ArtworkUri = null);
