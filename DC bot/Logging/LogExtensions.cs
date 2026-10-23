using Microsoft.Extensions.Logging;

namespace DC_bot.Logging;

/// <summary>
/// Defines structured log events shared by command, Discord, and playback boundaries.
/// </summary>
public static partial class LogExtensions
{

    /// <summary>
    /// Writes the structured log event: Command invoked: {CommandName}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Command invoked: {CommandName}")]
    public static partial void CommandInvoked(this ILogger logger, string commandName);

    /// <summary>
    /// Writes the structured log event: Command executed: {CommandName}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug, Message = "Command executed: {CommandName}")]
    public static partial void CommandExecuted(this ILogger logger, string commandName);

    /// <summary>
    /// Writes the structured log event: Command execution failed: {CommandName}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    [LoggerMessage(EventId = 1004, Level = LogLevel.Error, Message = "Command execution failed: {CommandName}")]
    public static partial void CommandExecutionFailed(this ILogger logger, Exception exception, string commandName);

    /// <summary>
    /// Writes the structured log event: CommandHandler Service is already registered.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "CommandHandler Service is already registered")]
    public static partial void CommandHandlerAlreadyRegistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Registered command handler.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1102, Level = LogLevel.Information, Message = "Registered command handler")]
    public static partial void CommandHandlerRegistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: No prefix provided.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1103, Level = LogLevel.Error, Message = "No prefix provided")]
    public static partial void CommandHandlerNoPrefix(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Unknown command. Use `!help` to see available commands..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1104, Level = LogLevel.Information, Message = "Unknown command. Use `!help` to see available commands.")]
    public static partial void CommandHandlerUnknownCommand(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Unregistered command handler.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1105, Level = LogLevel.Information, Message = "Unregistered command handler")]
    public static partial void CommandHandlerUnregistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Tried to unregister handler, but it was not registered.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1106, Level = LogLevel.Warning, Message = "Tried to unregister handler, but it was not registered")]
    public static partial void CommandHandlerUnregisterNotRegistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: ReactionHandler Service is already registered.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1201, Level = LogLevel.Information, Message = "ReactionHandler Service is already registered")]
    public static partial void ReactionHandlerAlreadyRegistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Registered reaction handler..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1202, Level = LogLevel.Information, Message = "Registered reaction handler.")]
    public static partial void ReactionHandlerRegistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Unregistered reaction handler.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1203, Level = LogLevel.Information, Message = "Unregistered reaction handler")]
    public static partial void ReactionHandlerUnregistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Tried to unregister handlers, but it was not registered.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1204, Level = LogLevel.Warning, Message = "Tried to unregister handlers, but it was not registered")]
    public static partial void ReactionHandlerUnregisterNotRegistered(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Reaction control message sent and reactions added..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1205, Level = LogLevel.Information, Message = "Reaction control message sent and reactions added.")]
    public static partial void ReactionControlMessageSent(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Reaction added: {Emoji} by {Username}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="emoji">The emoji supplied for this operation.</param>
    /// <param name="username">The username supplied for this operation.</param>
    [LoggerMessage(EventId = 1206, Level = LogLevel.Information, Message = "Reaction added: {Emoji} by {Username}")]
    public static partial void ReactionAdded(this ILogger logger, string emoji, string username);

    /// <summary>
    /// Writes the structured log event: Reaction removed: {Emoji} by {Username}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="emoji">The emoji supplied for this operation.</param>
    /// <param name="username">The username supplied for this operation.</param>
    [LoggerMessage(EventId = 1207, Level = LogLevel.Information, Message = "Reaction removed: {Emoji} by {Username}")]
    public static partial void ReactionRemoved(this ILogger logger, string emoji, string username);

    /// <summary>
    /// Writes the structured log event: Reaction handler operation failed: {Operation}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    [LoggerMessage(EventId = 1208, Level = LogLevel.Error, Message = "Reaction handler operation failed: {Operation}")]
    public static partial void ReactionHandlerOperationFailed(this ILogger logger, Exception exception, string operation);

    /// <summary>
    /// Writes the structured log event: Reaction handler message send failed: {Operation}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    [LoggerMessage(EventId = 1209, Level = LogLevel.Error, Message = "Reaction handler message send failed: {Operation}")]
    public static partial void ReactionHandlerMessageSendFailed(this ILogger logger, Exception exception, string operation);

    /// <summary>
    /// Writes the structured log event: Loading localization for {LanguageCode}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="languageCode">The language code supplied for this operation.</param>
    [LoggerMessage(EventId = 1301, Level = LogLevel.Information, Message = "Loading localization for {LanguageCode}")]
    public static partial void LocalizationLoading(this ILogger logger, string languageCode);

    /// <summary>
    /// Writes the structured log event: Localization loaded..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1302, Level = LogLevel.Information, Message = "Localization loaded.")]
    public static partial void LocalizationLoaded(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Localization read failed: {FilePath}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="filePath">The file path supplied for this operation.</param>
    [LoggerMessage(EventId = 1303, Level = LogLevel.Error, Message = "Localization read failed: {FilePath}")]
    public static partial void LocalizationReadFailed(this ILogger logger, Exception exception, string filePath);

    /// <summary>
    /// Writes the structured log event: Localization write failed: {FilePath}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="filePath">The file path supplied for this operation.</param>
    [LoggerMessage(EventId = 1304, Level = LogLevel.Error, Message = "Localization write failed: {FilePath}")]
    public static partial void LocalizationWriteFailed(this ILogger logger, Exception exception, string filePath);

    /// <summary>
    /// Writes the structured log event: User is Bot..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1403, Level = LogLevel.Information, Message = "User is Bot.")]
    public static partial void ValidationUserIsBot(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: User is not in a voice channel..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1404, Level = LogLevel.Information, Message = "User is not in a voice channel.")]
    public static partial void ValidationUserNotInVoiceChannel(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Bot is ready!.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1502, Level = LogLevel.Information, Message = "Bot is ready!")]
    public static partial void DiscordClientReady(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Guild available: {GuildName}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="guildName">The guild name supplied for this operation.</param>
    [LoggerMessage(EventId = 1503, Level = LogLevel.Information, Message = "Guild available: {GuildName}")]
    public static partial void DiscordClientGuildAvailable(this ILogger logger, string guildName);

    /// <summary>
    /// Writes the structured log event: Discord client event failed: {EventName}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="eventName">The event name supplied for this operation.</param>
    [LoggerMessage(EventId = 1504, Level = LogLevel.Error, Message = "Discord client event failed: {EventName}")]
    public static partial void DiscordClientEventFailed(this ILogger logger, Exception exception, string eventName);

    /// <summary>
    /// Writes the structured log event: Starting playing a music through URL..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1601, Level = LogLevel.Information, Message = "Starting playing a music through URL.")]
    public static partial void PlayCommandStartUrl(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Starting playing a music through search result..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1602, Level = LogLevel.Information, Message = "Starting playing a music through search result.")]
    public static partial void PlayCommandStartQuery(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Queue is empty. Playback has stopped..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 1701, Level = LogLevel.Information, Message = "Queue is empty. Playback has stopped.")]
    public static partial void QueueIsEmpty(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Lavalink node connected successfully.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Lavalink node connected successfully")]
    public static partial void LavalinkNodeConnectedSuccessfully(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Lavalink connection failed: {Message}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "Lavalink connection failed: {Message}")]
    public static partial void LavalinkConnectionFailed(this ILogger logger, Exception exception, string message);

    /// <summary>
    /// Writes the structured log event: There is no track currently playing..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 2006, Level = LogLevel.Information, Message = "There is no track currently playing.")]
    public static partial void ThereIsNoTrackCurrentlyPlaying(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: There is no track currently paused..
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    [LoggerMessage(EventId = 2007, Level = LogLevel.Information, Message = "There is no track currently paused.")]
    public static partial void ThereIsNoTrackCurrentlyPaused(this ILogger logger);

    /// <summary>
    /// Writes the structured log event: Now Playing: {Author} - {Title}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="author">The author supplied for this operation.</param>
    /// <param name="title">The title supplied for this operation.</param>
    [LoggerMessage(EventId = 2011, Level = LogLevel.Information, Message = "Now Playing: {Author} - {Title}")]
    public static partial void NowPlaying(this ILogger logger, string author, string title);

    /// <summary>
    /// Writes the structured log event: Lavalink operation failed: {Operation}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    [LoggerMessage(EventId = 2013, Level = LogLevel.Error, Message = "Lavalink operation failed: {Operation}")]
    public static partial void LavalinkOperationFailed(this ILogger logger, Exception exception, string operation);

    /// <summary>
    /// Writes the structured log event: Response send failed: {Operation}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    [LoggerMessage(EventId = 3001, Level = LogLevel.Error, Message = "Response send failed: {Operation}")]
    public static partial void ResponseSendFailed(this ILogger logger, Exception exception, string operation);

    /// <summary>
    /// Writes the structured log event: Message send failed: {Operation}.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="exception">The exception supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    [LoggerMessage(EventId = 3002, Level = LogLevel.Error, Message = "Message send failed: {Operation}")]
    public static partial void MessageSendFailed(this ILogger logger, Exception exception, string operation);
}
