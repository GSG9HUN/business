namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

/// <summary>Describes saved playlist metadata, including an optional explicitly configured cover.</summary>
/// <param name="Id">The playlist identifier.</param>
/// <param name="GuildId">The owning guild identifier.</param>
/// <param name="Name">The playlist name.</param>
/// <param name="ArtworkUri">The custom cover URL, or null for automatic artwork.</param>
public sealed record PlaylistRecord(
    long Id,
    ulong GuildId,
    string Name,
    string? ArtworkUri = null);
