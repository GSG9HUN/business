# Event ID Reference Table

This reference lists the 38 source-generated `LoggerMessage` declarations in `LogExtensions.cs`, including their exact event IDs, levels, method names, and message templates.

It covers these application declarations only. Direct `ILogger.Log*` calls, API or persistence logging, and DSharpPlus/Lavalink4NET events are not a complete part of this catalog. A declaration does not guarantee that its method is called or that the configured filter displays it. Gaps between IDs do not imply missing declarations.

## Command Events (1000-1099)

| EventID | Level | Method                   | Message                                 |
|--------:|-------|--------------------------|-----------------------------------------|
|    1001 | Debug | `CommandInvoked`         | Command invoked: {CommandName}          |
|    1002 | Debug | `CommandExecuted`        | Command executed: {CommandName}         |
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
|    1208 | Error       | `ReactionHandlerOperationFailed`         | Reaction handler operation failed: {Operation}          |
|    1209 | Error       | `ReactionHandlerMessageSendFailed`       | Reaction handler message send failed: {Operation}       |

**Usage:** Track emoji reaction events, control message lifecycle, and reaction handler failures.

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
|    1403 | Information | `ValidationUserIsBot`             | User is Bot.                             |
|    1404 | Information | `ValidationUserNotInVoiceChannel` | User is not in a voice channel.          |

**Usage:** Track validation failures for debugging command issues.

## Discord Client Events (1500-1599)

| EventID | Level       | Method                           | Message                                        |
|--------:|-------------|----------------------------------|------------------------------------------------|
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
|    2006 | Information | `ThereIsNoTrackCurrentlyPlaying`    | There is no track currently playing.                |
|    2007 | Information | `ThereIsNoTrackCurrentlyPaused`     | There is no track currently paused.                 |
|    2011 | Information | `NowPlaying`                        | Now Playing: {Author} - {Title}                     |
|    2013 | Error       | `LavalinkOperationFailed`           | Lavalink operation failed: {Operation}              |

**Usage:** Track audio server connection and playback state.

## Response Events (3000-3099)

| EventID | Level | Method               | Message                           |
|--------:|-------|----------------------|-----------------------------------|
|    3001 | Error | `ResponseSendFailed` | Response send failed: {Operation} |
|    3002 | Error | `MessageSendFailed`  | Message send failed: {Operation}  |

**Usage:** Track Discord API response failures.

## Event ID Ranges

| Range | Domain | Purpose |
|---|---|---|
| 1000-1099 | Command execution | Invocation, completion, and execution failure. |
| 1100-1199 | Command handler | Registration, routing diagnostics, and unregistration. |
| 1200-1299 | Reactions | Reaction controls, message setup, and failures. |
| 1300-1399 | Localization | Translation loading and file access failures. |
| 1400-1499 | Validation | Bot-user and voice-membership rejection. |
| 1500-1599 | Discord client | Ready, guild availability, and event-handler failures. |
| 1600-1699 | Play command | URL and query playback initiation. |
| 1700-1799 | Queue status | Empty-queue notification. |
| 2000-2099 | Lavalink | Node connectivity and playback diagnostics. |
| 3000-3099 | Responses | Discord response-delivery failures. |

These ranges describe this application's extension methods. An event ID from a framework or third-party logger must also be interpreted together with its category; matching numeric values do not make it the same application event.

## Current Startup Configuration

`DC bot/Startup/DependencyInjection/LoggingServiceCollectionExtensions.cs` configures the simple console provider, scope inclusion, UTC timestamps, and category filters:

```csharp
services.AddLogging(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        options.UseUtcTimestamp = true;
    })
        .SetMinimumLevel(LogLevel.Warning)
        .AddFilter("DC_bot.Service.Core.ValidationService", LogLevel.Debug)
        .AddFilter("DC_bot.Wrapper.Discord.DiscordClientEventHandler", LogLevel.Information)
        .AddFilter("DC_bot.Service.Music.Lifecycle", LogLevel.Information)
        .AddFilter("DSharpPlus", LogLevel.Debug)
        .AddFilter("Lavalink4NET", LogLevel.Debug);
});
```

| Category | Minimum level |
|---|---|
| Categories without a matching override | Warning |
| `DC_bot.Service.Core.ValidationService` | Debug |
| `DC_bot.Wrapper.Discord.DiscordClientEventHandler` | Information |
| `DC_bot.Service.Music.Lifecycle` | Information |
| `DSharpPlus` | Debug |
| `Lavalink4NET` | Debug |

The event level comes from its declaration, while visibility depends on the category of the calling logger. Calling an extension method does not change that category to `DC_bot.Logging`. For example, command invocation and completion events are Debug and are normally hidden by the default Warning threshold. Their absence alone does not prove that a command was never executed.

## Usage Examples

### Command Scope and Events

With the caller's `ILogger` and Discord context values available:

```csharp
using (logger.BeginCommandScope(commandName, userId, channelId, guildId))
{
    logger.CommandInvoked(commandName);
    // Execute the command through its normal pipeline.
    logger.CommandExecuted(commandName);
}
```

`BeginCommandScope` attaches `CommandName`, `UserId`, `ChannelId`, and `GuildId`. Scope rendering depends on the logging provider; including scopes does not define a separate log-entry model or guarantee a particular rendering of dictionary values.

### Troubleshooting

- **Command failure:** Event 1004 reports command execution failure; 1001 and 1002 can show invocation/completion when Debug is enabled for the command's logger category.
- **Discord event failure:** Event 1504 identifies the failed callback by `EventName`; inspect its exception and surrounding connection or voice logs.
- **Music failure:** Events 2002 and 2013 report node-connection and named Lavalink operation failures. Event 2001 confirms a node connection, not a guild player's voice readiness.
- **Queue end:** Event 1701 reports the empty-queue path. Investigate direct queue, playback, and lifecycle logs for details not represented by a catalogued extension method.
- **Response failure:** Events 3001 and 3002 identify response-delivery operations; a failed reply does not independently establish whether remote playback succeeded.

### Correlation Boundaries

`CommandName` groups executions of the same command; it is not a unique execution ID. Several users can invoke `play` concurrently. Use the available guild, user, and channel context together with the relevant log category and timestamps. For lifecycle operations that explicitly log `OperationId`, correlate messages by that operation ID as well.

These extension declarations do not define universal `Duration`, `Timestamp`, or `logs` properties. Performance analysis requires actual timing fields emitted by the operation or an explicitly defined external log/export model.

## Related Components

- `DC bot/Logging/README.md` — Logging components, method signatures, and usage examples.
- `DC bot/Logging/LogExtensions.cs` — Source of the event IDs, levels, and message templates listed here.
- `DC bot/Logging/LoggingScopes.cs` — Command and Discord scope fields.
- `DC bot/Startup/DependencyInjection/LoggingServiceCollectionExtensions.cs` — Provider setup and category-specific visibility rules.
