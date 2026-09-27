using DC_bot.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

internal static class QueueRealtimeNotifier
{
    private const string QueueUpdatesChannel = "queue_updates";
    
    public static Task NotifyQueueUpdatedAsync(
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

        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_notify({QueueUpdatesChannel}, {payload});",
            cancellationToken);
    }
}
