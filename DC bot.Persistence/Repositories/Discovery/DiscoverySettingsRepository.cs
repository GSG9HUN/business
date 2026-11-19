using DC_bot.Db;
using DC_bot.Entities.Discovery;
using DC_bot.Interface.Service.Persistence.Discovery;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Discovery;

/// <summary>Persists discovery settings without changing unrelated guild or mobile preferences.</summary>
/// <param name="factory">Creates independent database contexts.</param>
public sealed class DiscoverySettingsRepository(IDbContextFactory<BotDbContext> factory) : IDiscoverySettingsRepository
{

    /// <inheritdoc />
    public async Task<bool> GetPickerAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.UserMusicSettings.Where(item => item.UserId == userId)
            .Select(item => item.SearchPickerEnabled)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task SetPickerAsync(ulong userId, bool enabled, CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
        {
            var item = await db.UserMusicSettings.SingleOrDefaultAsync(item => item.UserId == userId, token);

            if (item is null)
            {
                item = new UserMusicSettingsEntity { UserId = userId };
                db.UserMusicSettings.Add(item);
            }

            item.SearchPickerEnabled = enabled;
            await db.SaveChangesAsync(token);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> GetAutoplayAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.GuildMusicSettings.Where(item => item.GuildId == guildId)
            .Select(item => item.AutoplayEnabled)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task SetAutoplayAsync(ulong guildId, bool enabled, CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
        {
            var item = await db.GuildMusicSettings.SingleOrDefaultAsync(item => item.GuildId == guildId, token);

            if (item is null)
            {
                item = new GuildMusicSettingsEntity { GuildId = guildId };
                db.GuildMusicSettings.Add(item);
            }

            item.AutoplayEnabled = enabled;
            await db.SaveChangesAsync(token);
        }, cancellationToken);
    }
}
