using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DC_bot.Logging;

public static class LoggingScopes
{
    public static IDisposable BeginCommandScope(this ILogger logger, string commandName, ulong? userId, ulong? channelId, ulong? guildId)
    {
        return logger.BeginScope(new Dictionary<string, object?>
        {
            ["CommandName"] = commandName,
            ["UserId"] = userId,
            ["ChannelId"] = channelId,
            ["GuildId"] = guildId
        });
    }
}
