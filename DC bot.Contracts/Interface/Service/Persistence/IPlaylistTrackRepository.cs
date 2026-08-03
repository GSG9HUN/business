using DC_bot.Interface.Service.Persistence.Models;

namespace DC_bot.Interface.Service.Persistence;

public interface IPlaylistTrackRepository
{
    Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdOrderedAsync(
        long playlistId,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        long playlistId,
        IReadOnlyCollection<PlaylistTrackCreateRecord> tracks,
        CancellationToken cancellationToken = default);

    Task AddTrackAsync(
        long playlistId,
        PlaylistTrackCreateRecord track,
        CancellationToken cancellationToken = default);

    Task RemoveTrackAsync(
        long playlistId,
        int orderNumber,
        CancellationToken cancellationToken = default);
}
