using DC_bot.Db;
using DC_bot.Entities.Playlists;
using DC_bot.Interface.Service.Persistence.Exceptions;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Playlists;

public class PlaylistRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IPlaylistRepository
{
    public async Task<PlaylistRecord?> GetByGuildAndNameAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.Playlists
            .AsNoTracking()
            .FirstOrDefaultAsync(playlist => playlist.GuildId == guildId && playlist.Name == playlistName,
                cancellationToken);

        return entity is null ? null : MapToRecord(entity);
    }

    public async Task<IReadOnlyList<PlaylistSummaryRecord>> GetByGuildAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Playlists
            .AsNoTracking()
            .Where(playlist => playlist.GuildId == guildId)
            .OrderBy(playlist => playlist.Name)
            .Select(playlist => new PlaylistSummaryRecord(
                playlist.Id,
                playlist.GuildId,
                playlist.Name,
                playlist.Tracks.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Playlists
            .AsNoTracking()
            .AnyAsync(playlist => playlist.GuildId == guildId && playlist.Name == playlistName, cancellationToken);
    }

    public async Task<bool> DeleteByGuildAndNameAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var affectedRows = await dbContext.Playlists
            .Where(playlist => playlist.GuildId == guildId && playlist.Name == playlistName)
            .ExecuteDeleteAsync(cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> RenameAsync(
        ulong guildId,
        string currentName,
        string newName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        int affectedRows;
        try
        {
            affectedRows = await dbContext.Playlists
                .Where(playlist => playlist.GuildId == guildId && playlist.Name == currentName)
                .ExecuteUpdateAsync(setters => setters.SetProperty(playlist => playlist.Name, newName), cancellationToken);
        }
        catch (Exception exception) when (PostgreSqlConcurrencyHelper.IsUniqueViolation(exception))
        {
            throw new UniqueConstraintConflictException(
                $"Playlist '{newName}' already exists for guild '{guildId}'.",
                exception);
        }

        return affectedRows > 0;
    }

    public async Task<long> CreatePlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var playlist = new PlaylistEntity
        {
            GuildId = guildId,
            Name = playlistName
        };

        dbContext.Playlists.Add(playlist);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (PostgreSqlConcurrencyHelper.IsUniqueViolation(exception))
        {
            throw new UniqueConstraintConflictException(
                $"Playlist '{playlistName}' already exists for guild '{guildId}'.",
                exception);
        }

        return playlist.Id;
    }

    private static PlaylistRecord MapToRecord(PlaylistEntity entity)
    {
        return new PlaylistRecord(
            entity.Id,
            entity.GuildId,
            entity.Name);
    }

}
