using DC_bot.Interface.Service.Persistence.Models.Discovery;

namespace DC_bot.Interface.Service.Persistence.Discovery;

/// <summary>Stores personal favorites with owner-scoped reads and mutations.</summary>
public interface IFavoriteRepository
{

    /// <summary>Adds or returns the existing favorite for the user's source identity.</summary>
    Task<FavoriteTrackRecord> AddAsync(
        ulong userId,
        string trackKey,
        string trackIdentifier,
        string title,
        string author,
        CancellationToken cancellationToken = default);

    /// <summary>Lists a bounded page of personal favorites in stable identifier order.</summary>
    Task<IReadOnlyList<FavoriteTrackRecord>> ListAsync(ulong userId, int page, CancellationToken cancellationToken = default);

    /// <summary>Gets a favorite only when it belongs to the requesting user.</summary>
    Task<FavoriteTrackRecord?> GetAsync(ulong userId, long id, CancellationToken cancellationToken = default);

    /// <summary>Removes a favorite only when it belongs to the requesting user.</summary>
    Task<bool> RemoveAsync(ulong userId, long id, CancellationToken cancellationToken = default);
}
