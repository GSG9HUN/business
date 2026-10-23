using DC_bot.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.GuildBotStatus;

/// <summary>
/// Publishes guild bot status realtime notifications within the caller&apos;s database transaction.
/// </summary>
internal static class GuildBotStatusRealtimeNotifier
{
    private const string GuildBotStatusUpdatesChannel = "guild_bot_status_updates";

    /// <summary>
    /// Publishes a guild voice-status event through the current database transaction when the provider is relational.
    /// </summary>
    /// <remarks>The notification is part of the current PostgreSQL transaction and becomes observable on commit. This method does not commit the caller&apos;s transaction.</remarks>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "eventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    public static Task NotifyGuildBotStatusUpdatedAsync(
        BotDbContext dbContext,
        ulong guildId,
        string eventName,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            return Task.CompletedTask;
        }

        var payload = $"{guildId}|{eventName}";
        return dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_notify({GuildBotStatusUpdatesChannel}, {payload});", cancellationToken);
    }
}
