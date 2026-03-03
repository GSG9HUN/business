using Microsoft.Extensions.Logging;

namespace DC_bot.Logging;

public static partial class LogExtensions
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Command invoked: {CommandName}")]
    public static partial void CommandInvoked(this ILogger logger, string commandName);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug, Message = "Command executed: {CommandName}")]
    public static partial void CommandExecuted(this ILogger logger, string commandName);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Command missing argument: {CommandName}")]
    public static partial void CommandMissingArgument(this ILogger logger, string commandName);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "CommandHandler Service is already registered")]
    public static partial void CommandHandlerAlreadyRegistered(this ILogger logger);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Information, Message = "Registered command handler")]
    public static partial void CommandHandlerRegistered(this ILogger logger);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Error, Message = "No prefix provided")]
    public static partial void CommandHandlerNoPrefix(this ILogger logger);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Information, Message = "Unknown command. Use `!help` to see available commands.")]
    public static partial void CommandHandlerUnknownCommand(this ILogger logger);

    [LoggerMessage(EventId = 1105, Level = LogLevel.Information, Message = "Unregistered command handler")]
    public static partial void CommandHandlerUnregistered(this ILogger logger);

    [LoggerMessage(EventId = 1106, Level = LogLevel.Warning, Message = "Tried to unregister handler, but it was not registered")]
    public static partial void CommandHandlerUnregisterNotRegistered(this ILogger logger);

    [LoggerMessage(EventId = 1201, Level = LogLevel.Information, Message = "ReactionHandler Service is already registered")]
    public static partial void ReactionHandlerAlreadyRegistered(this ILogger logger);

    [LoggerMessage(EventId = 1202, Level = LogLevel.Information, Message = "Registered reaction handler.")]
    public static partial void ReactionHandlerRegistered(this ILogger logger);

    [LoggerMessage(EventId = 1203, Level = LogLevel.Information, Message = "Unregistered reaction handler")]
    public static partial void ReactionHandlerUnregistered(this ILogger logger);

    [LoggerMessage(EventId = 1204, Level = LogLevel.Warning, Message = "Tried to unregister handlers, but it was not registered")]
    public static partial void ReactionHandlerUnregisterNotRegistered(this ILogger logger);

    [LoggerMessage(EventId = 1205, Level = LogLevel.Information, Message = "Reaction control message sent and reactions added.")]
    public static partial void ReactionControlMessageSent(this ILogger logger);

    [LoggerMessage(EventId = 1206, Level = LogLevel.Information, Message = "Reaction added: {Emoji} by {Username}")]
    public static partial void ReactionAdded(this ILogger logger, string emoji, string username);

    [LoggerMessage(EventId = 1207, Level = LogLevel.Information, Message = "Reaction removed: {Emoji} by {Username}")]
    public static partial void ReactionRemoved(this ILogger logger, string emoji, string username);

    [LoggerMessage(EventId = 1301, Level = LogLevel.Information, Message = "Loading localization for {LanguageCode}")]
    public static partial void LocalizationLoading(this ILogger logger, string languageCode);

    [LoggerMessage(EventId = 1302, Level = LogLevel.Information, Message = "Localization loaded.")]
    public static partial void LocalizationLoaded(this ILogger logger);

    [LoggerMessage(EventId = 1401, Level = LogLevel.Information, Message = "Lavalink is not connected.")]
    public static partial void ValidationLavalinkNotConnected(this ILogger logger);

    [LoggerMessage(EventId = 1402, Level = LogLevel.Information, Message = "Bot is not connected to a voice channel.")]
    public static partial void ValidationBotNotConnected(this ILogger logger);

    [LoggerMessage(EventId = 1403, Level = LogLevel.Information, Message = "User is Bot.")]
    public static partial void ValidationUserIsBot(this ILogger logger);

    [LoggerMessage(EventId = 1404, Level = LogLevel.Information, Message = "User is not in a voice channel.")]
    public static partial void ValidationUserNotInVoiceChannel(this ILogger logger);

    [LoggerMessage(EventId = 1501, Level = LogLevel.Information, Message = "Logger initialized for SingletonDiscordClient.")]
    public static partial void DiscordClientLoggerInitialized(this ILogger logger);

    [LoggerMessage(EventId = 1502, Level = LogLevel.Information, Message = "Bot is ready!")]
    public static partial void DiscordClientReady(this ILogger logger);

    [LoggerMessage(EventId = 1503, Level = LogLevel.Information, Message = "Guild available: {GuildName}")]
    public static partial void DiscordClientGuildAvailable(this ILogger logger, string guildName);

    [LoggerMessage(EventId = 1601, Level = LogLevel.Information, Message = "Starting playing a music through URL.")]
    public static partial void PlayCommandStartUrl(this ILogger logger);

    [LoggerMessage(EventId = 1602, Level = LogLevel.Information, Message = "Starting playing a music through search result.")]
    public static partial void PlayCommandStartQuery(this ILogger logger);

    [LoggerMessage(EventId = 1701, Level = LogLevel.Information, Message = "Queue is empty. Playback has stopped.")]
    public static partial void QueueIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Lavalink node connected successfully")]
    public static partial void LavalinkNodeConnectedSuccessfully(this ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "Lavalink connection failed: {Message}")]
    public static partial void LavalinkConnectionFailed(this ILogger logger, Exception exception, string message);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "PlaybackFinished event registered.")]
    public static partial void PlaybackFinishedEventRegistered(this ILogger logger);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Failed to find music with url: {Url}")]
    public static partial void FailedToFindMusicWithUrl(this ILogger logger, string url);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "Failed to find music with query: {Query}")]
    public static partial void FailedToFindMusicWithQuery(this ILogger logger, string query);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Information, Message = "There is no track currently playing.")]
    public static partial void ThereIsNoTrackCurrentlyPlaying(this ILogger logger);

    [LoggerMessage(EventId = 2007, Level = LogLevel.Information, Message = "There is no track currently paused.")]
    public static partial void ThereIsNoTrackCurrentlyPaused(this ILogger logger);

    [LoggerMessage(EventId = 2008, Level = LogLevel.Information, Message = "Added to queue.")]
    public static partial void AddedToQueue(this ILogger logger);

    [LoggerMessage(EventId = 2009, Level = LogLevel.Information, Message = "Added to queue: {Author} - {Title}")]
    public static partial void AddedToQueueWithDetails(this ILogger logger, string author, string title);

    [LoggerMessage(EventId = 2011, Level = LogLevel.Information, Message = "Now Playing: {Author} - {Title}")]
    public static partial void NowPlaying(this ILogger logger, string author, string title);

    [LoggerMessage(EventId = 2012, Level = LogLevel.Information, Message = "Repeating: {RepeatTrackAuthor} - {RepeatTrackTitle}")]
    public static partial void Repeating(this ILogger logger, string repeatTrackAuthor, string repeatTrackTitle);
}
