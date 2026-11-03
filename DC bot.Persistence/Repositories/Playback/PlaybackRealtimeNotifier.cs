using DC_bot.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Playback;

/// <summary>
/// Publishes playback realtime notifications within the caller&apos;s database transaction.
/// </summary>
internal static class PlaybackRealtimeNotifier
{
    private const string PlaybackUpdatesChannel = "playback_state_updates";

    /// <summary>
    /// Publishes a playback event through the current database transaction; non-relational test providers are skipped.
    /// </summary>
    /// <remarks>The notification is part of the current PostgreSQL transaction and becomes observable on commit. This method does not commit the caller&apos;s transaction.</remarks>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "eventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    public static Task NotifyPlaybackUpdatedAsync(
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
        return dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_notify({PlaybackUpdatesChannel}, {payload});", cancellationToken);
    }
}
