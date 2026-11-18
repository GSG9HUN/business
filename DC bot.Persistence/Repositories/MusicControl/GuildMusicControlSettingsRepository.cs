using DC_bot.Db;
using DC_bot.Entities.MusicControl;
using DC_bot.Interface.Service.Persistence.Models.MusicControl;
using DC_bot.Interface.Service.Persistence.MusicControl;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.MusicControl;

/// <summary>Updates tracked music-control settings with serializable conflict retries.</summary>
/// <param name="factory">Creates independently owned database contexts.</param>
public sealed class GuildMusicControlSettingsRepository(IDbContextFactory<BotDbContext> factory)
    : IGuildMusicControlSettingsRepository
{

    /// <inheritdoc />
    public async Task<GuildMusicControlSettingsRecord> GetAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.GuildMusicControlSettings.AsNoTracking()
            .Where(item => item.GuildId == guildId)
            .Select(item => new GuildMusicControlSettingsRecord(item.VoteSkipEnabled, item.DjRoleId))
            .SingleOrDefaultAsync(cancellationToken) ?? new(false, null);
    }

    /// <inheritdoc />
    public Task SetVoteSkipAsync(ulong guildId, bool enabled, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(guildId, item => item.VoteSkipEnabled = enabled, cancellationToken);
    }

    /// <inheritdoc />
    public Task SetDjRoleAsync(ulong guildId, ulong? roleId, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(guildId, item => item.DjRoleId = roleId, cancellationToken);
    }

    /// <summary>Changes only the requested tracked fields while retrying competing inserts or updates.</summary>
    private Task UpdateAsync(
        ulong guildId,
        Action<GuildMusicControlSettingsEntity> update,
        CancellationToken cancellationToken)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
        {
            var item = await db.GuildMusicControlSettings.SingleOrDefaultAsync(item => item.GuildId == guildId, token);

            if (item is null)
            {
                item = new GuildMusicControlSettingsEntity { GuildId = guildId };
                db.GuildMusicControlSettings.Add(item);
            }

            update(item);
            await db.SaveChangesAsync(token);
        }, cancellationToken);
    }
}
