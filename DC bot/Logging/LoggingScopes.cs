using Microsoft.Extensions.Logging;

namespace DC_bot.Logging;

/// <summary>
/// Attaches command and guild identifiers to the logging scope of an operation.
/// </summary>
public static class LoggingScopes
{

    /// <summary>
    /// Creates a logging scope containing command and Discord context identifiers.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="userId">The user id supplied for this operation.</param>
    /// <param name="channelId">The channel id supplied for this operation.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static IDisposable? BeginCommandScope(
        this ILogger logger,
        string commandName,
        ulong? userId,
        ulong? channelId,
        ulong? guildId)
    {
        return logger.BeginScope(new Dictionary<string, object?> { ["CommandName"] = commandName, ["UserId"] = userId, ["ChannelId"] = channelId, ["GuildId"] = guildId });
    }
}
