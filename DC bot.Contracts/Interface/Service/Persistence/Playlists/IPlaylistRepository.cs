using DC_bot.Interface.Service.Persistence.Models.Playlists;

namespace DC_bot.Interface.Service.Persistence.Playlists;

/// <summary>
/// Defines shared storage operations for playlist repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IPlaylistRepository
{

    /// <summary>
    /// Reads the guild-owned playlist with the requested stored name.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "playlistName">The stored guild-owned playlist name.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<PlaylistRecord?> GetByGuildAndNameAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a queue item or playlist by its durable database identifier.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<PlaylistRecord?> GetByIdAsync(long playlistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the guild&apos;s saved playlists with their persisted track counts.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<PlaylistSummaryRecord>> GetByGuildAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the guild already owns a playlist with the specified name.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "playlistName">The stored guild-owned playlist name.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> ExistsAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes only the playlist matching both guild identity and stored name.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "playlistName">The stored guild-owned playlist name.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> DeleteByGuildAndNameAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the playlist with the specified durable identifier and reports whether a row was removed.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> DeleteByIdAsync(long playlistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames the guild-owned playlist and reports whether a matching row was changed.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "currentName">The current name value used by this storage contract.</param>
    /// <param name = "newName">The new name value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> RenameAsync(ulong guildId, string currentName, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames the identified playlist, translating a unique-name conflict into the documented repository conflict.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "newName">The new name value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> RenameByIdAsync(long playlistId, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a guild-owned empty playlist and returns its durable identifier, reporting duplicate-name conflicts.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "playlistName">The stored guild-owned playlist name.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<long> CreatePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
}
