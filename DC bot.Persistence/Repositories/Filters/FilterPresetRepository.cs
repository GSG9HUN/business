using DC_bot.Db;
using DC_bot.Entities.Filters;
using DC_bot.Interface.Service.Persistence.Filters;
using DC_bot.Interface.Service.Persistence.Models.Filters;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Filters;

/// <summary>Stores guild-local presets with serializable conflict handling and explicit overwrite semantics.</summary>
/// <param name="factory">Creates independent EF contexts.</param>
public sealed class FilterPresetRepository(IDbContextFactory<BotDbContext> factory) : IFilterPresetRepository
{

    /// <inheritdoc />
    public Task<bool> SaveAsync(
        ulong guildId,
        string name,
        int schemaVersion,
        string definitionJson,
        ulong updatedBy,
        bool overwrite,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
        {
            var item = await db.FilterPresets.SingleOrDefaultAsync(value => value.GuildId == guildId && value.Name == name, token);

            if (item is not null && !overwrite)
            {
                return false;
            }

            if (item is null)
            {
                item = new FilterPresetEntity { GuildId = guildId, Name = name };
                db.FilterPresets.Add(item);
            }

            item.SchemaVersion = schemaVersion;
            item.DefinitionJson = definitionJson;
            item.UpdatedBy = updatedBy;
            item.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return true;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FilterPresetRecord?> GetAsync(ulong guildId, string name, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var item = await db.FilterPresets.AsNoTracking()
            .SingleOrDefaultAsync(value => value.GuildId == guildId && value.Name == name, cancellationToken);
        return item is null ? null : Map(item);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FilterPresetRecord>> ListAsync(
        ulong guildId,
        int page,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var items = await db.FilterPresets.AsNoTracking()
            .Where(item => item.GuildId == guildId)
            .OrderBy(item => item.Name)
            .Skip(checked((page - 1) * 10))
            .Take(10)
            .ToListAsync(cancellationToken);
        return items.Select(Map)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(ulong guildId, string name, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var item = await db.FilterPresets.SingleOrDefaultAsync(
            value => value.GuildId == guildId && value.Name == name, cancellationToken);

        if (item is null)
        {
            return false;
        }

        db.FilterPresets.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Maps tracked values to the SDK-independent contract.</summary>
    private static FilterPresetRecord Map(FilterPresetEntity item) =>
        new(item.Name, item.SchemaVersion, item.DefinitionJson, item.UpdatedBy, item.UpdatedAtUtc);
}
