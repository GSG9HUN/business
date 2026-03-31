# Event ID Reference Table

This table documents all `LoggerMessage` EventIDs used throughout the application with their corresponding log levels
and messages.

## Command Events (1000-1099)

| EventID | Level | Method                   | Message                                 |
|--------:|-------|--------------------------|-----------------------------------------|
|    1001 | Debug | `CommandInvoked`         | Command invoked: {CommandName}          |
|    1002 | Debug | `CommandExecuted`        | Command executed: {CommandName}         |
|    1003 | Debug | `CommandMissingArgument` | Command missing argument: {CommandName} |
|    1004 | Error | `CommandExecutionFailed` | Command execution failed: {CommandName} |

**Usage:** Log every command invocation and completion for tracking user interactions.

## Command Handler Events (1100-1199)

| EventID | Level       | Method                                  | Message                                                 |
|--------:|-------------|-----------------------------------------|---------------------------------------------------------|
|    1101 | Information | `CommandHandlerAlreadyRegistered`       | CommandHandler Service is already registered            |
|    1102 | Information | `CommandHandlerRegistered`              | Registered command handler                              |
|    1103 | Error       | `CommandHandlerNoPrefix`                | No prefix provided                                      |
|    1104 | Information | `CommandHandlerUnknownCommand`          | Unknown command. Use `!help` to see available commands. |
|    1105 | Information | `CommandHandlerUnregistered`            | Unregistered command handler                            |
|    1106 | Warning     | `CommandHandlerUnregisterNotRegistered` | Tried to unregister handler, but it was not registered  |

**Usage:** Lifecycle events for command handler registration/unregistration.

## Reaction Handler Events (1200-1299)

| EventID | Level       | Method                                   | Message                                                 |
|--------:|-------------|------------------------------------------|---------------------------------------------------------|
|    1201 | Information | `ReactionHandlerAlreadyRegistered`       | ReactionHandler Service is already registered           |
|    1202 | Information | `ReactionHandlerRegistered`              | Registered reaction handler.                            |
|    1203 | Information | `ReactionHandlerUnregistered`            | Unregistered reaction handler                           |
|    1204 | Warning     | `ReactionHandlerUnregisterNotRegistered` | Tried to unregister handlers, but it was not registered |
|    1205 | Information | `ReactionControlMessageSent`             | Reaction control message sent and reactions added.      |
|    1206 | Information | `ReactionAdded`                          | Reaction added: {Emoji} by {Username}                   |
|    1207 | Information | `ReactionRemoved`                        | Reaction removed: {Emoji} by {Username}                 |

**Usage:** Track emoji reaction events and control message lifecycle.

## Localization Events (1300-1399)

| EventID | Level       | Method                    | Message                                 |
|--------:|-------------|---------------------------|-----------------------------------------|
|    1301 | Information | `LocalizationLoading`     | Loading localization for {LanguageCode} |
|    1302 | Information | `LocalizationLoaded`      | Localization loaded.                    |
|    1303 | Error       | `LocalizationReadFailed`  | Localization read failed: {FilePath}    |
|    1304 | Error       | `LocalizationWriteFailed` | Localization write failed: {FilePath}   |

**Usage:** Monitor language file loading and persistence.

## Validation Events (1400-1499)

| EventID | Level       | Method                            | Message                                  |
|--------:|-------------|-----------------------------------|------------------------------------------|
|    1401 | Information | `ValidationLavalinkNotConnected`  | Lavalink is not connected.               |
|    1402 | Information | `ValidationBotNotConnected`       | Bot is not connected to a voice channel. |
|    1403 | Information | `ValidationUserIsBot`             | User is Bot.                             |
|    1404 | Information | `ValidationUserNotInVoiceChannel` | User is not in a voice channel.          |

**Usage:** Track validation failures for debugging command issues.

## Discord Client Events (1500-1599)

| EventID | Level       | Method                           | Message                                        |
|--------:|-------------|----------------------------------|------------------------------------------------|
|    1501 | Information | `DiscordClientLoggerInitialized` | Logger initialized for SingletonDiscordClient. |
|    1502 | Information | `DiscordClientReady`             | Bot is ready!                                  |
|    1503 | Information | `DiscordClientGuildAvailable`    | Guild available: {GuildName}                   |
|    1504 | Error       | `DiscordClientEventFailed`       | Discord client event failed: {EventName}       |

**Usage:** Track bot lifecycle and Discord connection status.

## Play Command Events (1600-1699)

| EventID | Level       | Method                  | Message                                         |
|--------:|-------------|-------------------------|-------------------------------------------------|
|    1601 | Information | `PlayCommandStartUrl`   | Starting playing a music through URL.           |
|    1602 | Information | `PlayCommandStartQuery` | Starting playing a music through search result. |

**Usage:** Log music playback initiation.

## Queue Events (1700-1799)

| EventID | Level       | Method         | Message                               |
|--------:|-------------|----------------|---------------------------------------|
|    1701 | Information | `QueueIsEmpty` | Queue is empty. Playback has stopped. |

**Usage:** Notify when queue becomes empty.

## Lavalink Events (2000-2099)

| EventID | Level       | Method                              | Message                                             |
|--------:|-------------|-------------------------------------|-----------------------------------------------------|
|    2001 | Information | `LavalinkNodeConnectedSuccessfully` | Lavalink node connected successfully                |
|    2002 | Error       | `LavalinkConnectionFailed`          | Lavalink connection failed: {Message}               |
|    2003 | Information | `PlaybackFinishedEventRegistered`   | PlaybackFinished event registered.                  |
|    2004 | Information | `FailedToFindMusicWithUrl`          | Failed to find music with url: {Url}                |
|    2005 | Information | `FailedToFindMusicWithQuery`        | Failed to find music with query: {Query}            |
|    2006 | Information | `ThereIsNoTrackCurrentlyPlaying`    | There is no track currently playing.                |
|    2007 | Information | `ThereIsNoTrackCurrentlyPaused`     | There is no track currently paused.                 |
|    2008 | Information | `AddedToQueue`                      | Added to queue.                                     |
|    2009 | Information | `AddedToQueueWithTrackDetails`      | Added to queue: {Author} - {Title}                  |
|    2011 | Information | `NowPlaying`                        | Now Playing: {Author} - {Title}                     |
|    2012 | Information | `Repeating`                         | Repeating: {RepeatTrackAuthor} - {RepeatTrackTitle} |
|    2013 | Error       | `LavalinkOperationFailed`           | Lavalink operation failed: {Operation}              |

**Usage:** Track audio server connection and playback state.

## Queue Persistence Events (2100-2199)

| EventID | Level | Method                 | Message                       |
|--------:|-------|------------------------|-------------------------------|
|    2101 | Error | `MusicQueueSaveFailed` | Queue save failed: {FilePath} |
|    2102 | Error | `MusicQueueLoadFailed` | Queue load failed: {FilePath} |

**Usage:** Monitor queue persistence operations.

## Response Events (3000-3099)

| EventID | Level | Method               | Message                           |
|--------:|-------|----------------------|-----------------------------------|
|    3001 | Error | `ResponseSendFailed` | Response send failed: {Operation} |
|    3002 | Error | `MessageSendFailed`  | Message send failed: {Operation}  |

**Usage:** Track Discord API response failures.

## Event ID Ranges

| Range     | Domain            | Purpose                                   |
|-----------|-------------------|-------------------------------------------|
| 1000-1099 | Command Execution | Track command invocations and completions |
| 1100-1199 | Command Handler   | Lifecycle and registration                |
| 1200-1299 | Reactions         | Emoji reactions and controls              |
| 1300-1399 | Localization      | Language file operations                  |
| 1400-1499 | Validation        | User and state validation                 |
| 1500-1599 | Discord Client    | Bot lifecycle and connectivity            |
| 1600-1699 | Play Command      | Music playback initiation                 |
| 1700-1799 | Queue Status      | Queue state changes                       |
| 2000-2099 | Lavalink          | Audio server operations                   |
| 2100-2199 | Persistence       | Data storage operations                   |
| 3000-3099 | Response          | Discord API responses                     |

## Filtering by Event ID

### In appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DC_bot": "Debug"
    },
    "EventFilter": {
      "2000": "true",  // Only Lavalink events
      "1000": "true"   // Only command events
    }
  }
}
```

### In Code

```csharp
// Get all Lavalink events
var lavalinkLogs = logs.Where(l => l.EventId.Id >= 2000 && l.EventId.Id < 2100);

// Get all errors
var errors = logs.Where(l => l.Level == LogLevel.Error);
```

## Usage Examples

### Filtering for Troubleshooting

**Bot not responding to commands:**

- Check EventID 1001 (CommandInvoked) - Command reached handler
- Check EventID 1004 (CommandExecutionFailed) - Command failed

**Music not playing:**

- Check EventID 2001 (LavalinkNodeConnectedSuccessfully) - Connection OK
- Check EventID 2004, 2005 (FailedToFindMusic*) - Track not found
- Check EventID 2013 (LavalinkOperationFailed) - Audio error

**Queue issues:**

- Check EventID 2101, 2102 (Queue Save/Load Failed) - Persistence error
- Check EventID 1701 (QueueIsEmpty) - Queue status

### Performance Analysis

```csharp
// Find slow operations
var slowOps = logs
    .Where(l => l.EventId.Id >= 2000 && l.EventId.Id < 2100)
    .Where(l => l.Duration > TimeSpan.FromSeconds(1));
```

### Event Correlation

```csharp
// Trace a single command execution
var commandId = "play";
var execution = logs
    .Where(l => l.Message.Contains(commandId))
    .OrderBy(l => l.Timestamp);
```

## Related

- **README.md** - Logging extensions guide
- **LogExtensions.cs** - Extension method implementation
- **LoggingScopes.cs** - Scope helper implementation

