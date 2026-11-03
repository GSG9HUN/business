using DC_bot.Db;
using DC_bot.Entities.Playback;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Playback;

/// <summary>
/// Reads and mutates repeat list state using short-lived contexts from the injected EF factory.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class RepeatListRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IRepeatListRepository
{
    private const int MaxRepeatListItemsPerGuild = 50;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.GuildRepeatListItems.AsNoTracking()
            .Where(item => item.GuildId == guildId)
            .OrderBy(item => item.Position)
            .Select(item => item.TrackIdentifier)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ReplaceAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackIdentifiers);

        if (trackIdentifiers.Count > MaxRepeatListItemsPerGuild)
        {
            throw new InvalidOperationException($"Repeat list cannot contain more than {MaxRepeatListItemsPerGuild} tracks.");
        }

        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(dbContextFactory, (dbContext, ct) => ReplaceAsync(dbContext, guildId, trackIdentifiers, ct), cancellationToken);
    }

    /// <summary>
    /// Replaces the guild&apos;s legacy repeat-list snapshot with the supplied ordered track identifiers.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackIdentifiers">The track identities in the requested durable order, preserving duplicate occurrences.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static async Task ReplaceAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken)
    {
        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);
        var existingItems = await dbContext.GuildRepeatListItems.Where(item => item.GuildId == guildId)
            .ToListAsync(cancellationToken);
        dbContext.GuildRepeatListItems.RemoveRange(existingItems);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (trackIdentifiers.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            var newItems = trackIdentifiers.Select((trackIdentifier, index) => new GuildRepeatListItemEntity { GuildId = guildId, Position = index, TrackIdentifier = trackIdentifier, AddedAtUtc = now })
                .ToList();
            dbContext.GuildRepeatListItems.AddRange(newItems);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var existingItems = await dbContext.GuildRepeatListItems.Where(item => item.GuildId == guildId)
            .ToListAsync(cancellationToken);

        if (existingItems.Count == 0)
        {
            return;
        }

        dbContext.GuildRepeatListItems.RemoveRange(existingItems);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
