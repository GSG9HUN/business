using System.Data;
using DC_bot.Db;
using DC_bot.Entities.Playlists;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Playlists;

public class PlaylistTrackRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IPlaylistTrackRepository
{
    private const int MaxRetries = 3;

    public async Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdOrderedAsync(
        long playlistId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entities = await dbContext.PlaylistTracks
            .AsNoTracking()
            .Where(track => track.PlaylistId == playlistId)
            .OrderBy(track => track.OrderNumber)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToRecord).ToList();
    }

    public async Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdsOrderedAsync(
        IReadOnlyCollection<long> playlistIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(playlistIds);
        if (playlistIds.Count == 0)
        {
            return [];
        }

        var distinctPlaylistIds = playlistIds.Distinct().ToList();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entities = await dbContext.PlaylistTracks
            .AsNoTracking()
            .Where(track => distinctPlaylistIds.Contains(track.PlaylistId))
            .OrderBy(track => track.PlaylistId)
            .ThenBy(track => track.OrderNumber)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToRecord).ToList();
    }

    public async Task AddRangeAsync(
        long playlistId,
        IReadOnlyCollection<PlaylistTrackCreateRecord> tracks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        if (tracks.Count == 0)
        {
            return;
        }

        var trackList = tracks.ToList();

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var tx =
                await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                var nextOrderNumber = await GetNextOrderNumberAsync(dbContext, playlistId, cancellationToken);

                var toInsert = new List<PlaylistTrackEntity>(trackList.Count);
                foreach (var track in trackList)
                {
                    toInsert.Add(new PlaylistTrackEntity
                    {
                        PlaylistId = playlistId,
                        OrderNumber = nextOrderNumber++,
                        Source = track.Source,
                        TrackIdentifier = track.TrackIdentifier,
                        TrackUri = track.TrackUri
                    });
                }

                await dbContext.PlaylistTracks.AddRangeAsync(toInsert, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (PostgreSqlConcurrencyHelper.IsRetriable(ex) && attempt < MaxRetries)
            {
                await tx.RollbackAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }

        throw new InvalidOperationException("Failed to save playlist after retries due to concurrent modifications.");
    }

    public async Task AddTrackAsync(
        long playlistId,
        PlaylistTrackCreateRecord track,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var tx =
                await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                var nextOrderNumber = await GetNextOrderNumberAsync(dbContext, playlistId, cancellationToken);

                var playlistTrackEntity = new PlaylistTrackEntity
                {
                    PlaylistId = playlistId,
                    OrderNumber = nextOrderNumber,
                    Source = track.Source,
                    TrackIdentifier = track.TrackIdentifier,
                    TrackUri = track.TrackUri
                };
                await dbContext.PlaylistTracks.AddAsync(playlistTrackEntity, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (PostgreSqlConcurrencyHelper.IsRetriable(ex) && attempt < MaxRetries)
            {
                await tx.RollbackAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }

        throw new InvalidOperationException("Failed to append track after retries due to concurrent modifications.");
    }

    public async Task RemoveTrackAsync(
        long playlistId,
        int orderNumber,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            await using var tx =
                await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var playlistTrackEntity = await dbContext.PlaylistTracks
                .FirstOrDefaultAsync(track => track.PlaylistId == playlistId && track.OrderNumber == orderNumber,
                    cancellationToken);

            if (playlistTrackEntity is null)
            {
                await tx.RollbackAsync(cancellationToken);
                return;
            }

            try
            {
                dbContext.PlaylistTracks.Remove(playlistTrackEntity);
                await dbContext.SaveChangesAsync(cancellationToken);

                await ReorderPlaylistInTwoPhasesAsync(dbContext, playlistId, orderNumber, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (PostgreSqlConcurrencyHelper.IsRetriable(ex) && attempt < MaxRetries)
            {
                await tx.RollbackAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }

        throw new InvalidOperationException("Failed to remove track after retries due to concurrent modifications.");
    }

    private static async Task<int> GetNextOrderNumberAsync(
        BotDbContext dbContext,
        long playlistId,
        CancellationToken cancellationToken = default)
    {
        var maxNumber = await dbContext.PlaylistTracks
            .Where(track => track.PlaylistId == playlistId)
            .Select(track => (int?)track.OrderNumber)
            .MaxAsync(cancellationToken);

        return (maxNumber ?? 0) + 1;
    }

    private static async Task ReorderPlaylistInTwoPhasesAsync(
        BotDbContext dbContext,
        long playlistId,
        int deletedOrderNumber,
        CancellationToken cancellationToken)
    {
        var tempPlaylistTrackEntities = await dbContext.PlaylistTracks
            .Where(track => track.PlaylistId == playlistId && track.OrderNumber > deletedOrderNumber)
            .OrderBy(track => track.OrderNumber)
            .ToListAsync(cancellationToken);

        tempPlaylistTrackEntities.ForEach(track => track.OrderNumber *= -1);
        await dbContext.SaveChangesAsync(cancellationToken);

        var playlistTracks = await dbContext.PlaylistTracks
            .Where(track => track.PlaylistId == playlistId && track.OrderNumber < 0)
            .OrderByDescending(track => track.OrderNumber)
            .ToListAsync(cancellationToken);

        foreach (var (track, index) in playlistTracks.Select((track, index) => (track, index)))
        {
            track.OrderNumber = deletedOrderNumber + index;
        }
    }

    private static PlaylistTrackRecord MapToRecord(PlaylistTrackEntity entity)
    {
        return new PlaylistTrackRecord(
            entity.Id,
            entity.PlaylistId,
            entity.OrderNumber,
            entity.Source,
            entity.TrackIdentifier,
            entity.TrackUri);
    }

}
