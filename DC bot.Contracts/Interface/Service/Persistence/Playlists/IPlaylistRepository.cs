using DC_bot.Interface.Service.Persistence.Models.Playlists;

namespace DC_bot.Interface.Service.Persistence.Playlists;

public interface IPlaylistRepository
{
    Task<PlaylistRecord?> GetByGuildAndNameAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default);

    Task<PlaylistRecord?> GetByIdAsync(
        long playlistId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlaylistSummaryRecord>> GetByGuildAsync(
        ulong guildId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByGuildAndNameAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(
        long playlistId,
        CancellationToken cancellationToken = default);

    Task<bool> RenameAsync(
        ulong guildId,
        string currentName,
        string newName,
        CancellationToken cancellationToken = default);

    Task<bool> RenameByIdAsync(
        long playlistId,
        string newName,
        CancellationToken cancellationToken = default);

    Task<long> CreatePlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default);
}
