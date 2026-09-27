using DC_bot.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.GuildBotStatus;

internal static class GuildBotStatusRealtimeNotifier
{
    private const string GuildBotStatusUpdatesChannel = "guild_bot_status_updates";

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

        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_notify({GuildBotStatusUpdatesChannel}, {payload});",
            cancellationToken);
    }
}
