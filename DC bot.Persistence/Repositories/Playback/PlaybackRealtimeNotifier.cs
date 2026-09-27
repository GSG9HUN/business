using DC_bot.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Playback;

internal static class PlaybackRealtimeNotifier
{
    private const string PlaybackUpdatesChannel = "playback_state_updates";

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

        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_notify({PlaybackUpdatesChannel}, {payload});",
            cancellationToken);
    }
}