using DC_bot.Db;
using DC_bot.Entities.Discovery;
using DC_bot.Interface.Service.Persistence.Discovery;
using DC_bot.Interface.Service.Persistence.Models.Discovery;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Discovery;

/// <summary>Persists personal favorites and prevents duplicate source identities transactionally.</summary>
/// <param name="factory">Creates independent database contexts.</param>
public sealed class FavoriteRepository(IDbContextFactory<BotDbContext> factory) : IFavoriteRepository
{

    /// <inheritdoc />
    public Task<FavoriteTrackRecord> AddAsync(
        ulong userId,
        string trackKey,
        string trackIdentifier,
        string title,
        string author,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
        {
            var item = await db.FavoriteTracks.SingleOrDefaultAsync(
                value => value.UserId == userId && value.TrackKey == trackKey, token);

            if (item is null)
            {
                item = new FavoriteTrackEntity
                {
                    UserId = userId, TrackKey = trackKey, TrackIdentifier = trackIdentifier,
                    Title = title, Author = author, AddedAtUtc = DateTimeOffset.UtcNow
                };
                db.FavoriteTracks.Add(item);
                await db.SaveChangesAsync(token);
            }

            return Map(item);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FavoriteTrackRecord>> ListAsync(
        ulong userId,
        int page,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var items = await db.FavoriteTracks.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Id)
            .Skip(checked((page - 1) * 10))
            .Take(10)
            .ToListAsync(cancellationToken);
        return items.Select(Map)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<FavoriteTrackRecord?> GetAsync(ulong userId, long id, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var item = await db.FavoriteTracks.AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserId == userId && item.Id == id, cancellationToken);
        return item is null ? null : Map(item);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(ulong userId, long id, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var item = await db.FavoriteTracks.SingleOrDefaultAsync(item => item.UserId == userId && item.Id == id, cancellationToken);

        if (item is null)
        {
            return false;
        }

        db.FavoriteTracks.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Maps a persisted favorite to its immutable public storage contract.</summary>
    private static FavoriteTrackRecord Map(FavoriteTrackEntity item) =>
        new(item.Id, item.TrackIdentifier, item.Title, item.Author);
}
