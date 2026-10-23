# Logging

Defines typed log messages and reusable scopes for bot operations.

## Overview

- LogExtensions uses source-generated logging methods; LoggingScopes attaches command, guild, channel, and user context where supplied.
- The configured logging filter controls visibility. Avoid relying on a debug message as the only evidence of a failure, and do not include credentials or voice tokens in new logs.

## Event ID Reference

**File:** `EventIdTable.md`

**Purpose:** Lists the current source-generated application event IDs, levels, method names, and exact message templates. It also explains the console provider, category filters, and available correlation fields. Direct logger calls and third-party event IDs are outside that catalog's declaration scope.

---

## Classes

### LogExtensions

**File:** `DC bot/Logging/LogExtensions.cs`

**Purpose:** Defines structured log events shared by command, Discord, and playback boundaries.

**Methods:**

- `CommandExecuted` — Writes the structured log event: Command executed: {CommandName}.

```csharp
public static partial void CommandExecuted(this ILogger logger, string commandName);
```

- `CommandExecutionFailed` — Writes the structured log event: Command execution failed: {CommandName}.

```csharp
public static partial void CommandExecutionFailed(this ILogger logger, Exception exception, string commandName);
```

- `CommandHandlerAlreadyRegistered` — Writes the structured log event: CommandHandler Service is already registered.

```csharp
public static partial void CommandHandlerAlreadyRegistered(this ILogger logger);
```

- `CommandHandlerNoPrefix` — Writes the structured log event: No prefix provided.

```csharp
public static partial void CommandHandlerNoPrefix(this ILogger logger);
```

- `CommandHandlerRegistered` — Writes the structured log event: Registered command handler.

```csharp
public static partial void CommandHandlerRegistered(this ILogger logger);
```

- `CommandHandlerUnknownCommand` — Writes the structured log event: Unknown command. Use `!help` to see available commands..

```csharp
public static partial void CommandHandlerUnknownCommand(this ILogger logger);
```

**Additional methods:** `CommandInvoked`, `CommandHandlerUnregistered`, `CommandHandlerUnregisterNotRegistered`, `ReactionHandlerAlreadyRegistered`, `ReactionHandlerRegistered`, `ReactionHandlerUnregistered`, `ReactionHandlerUnregisterNotRegistered`, `ReactionControlMessageSent`, `ReactionAdded`, `ReactionRemoved`, `ReactionHandlerOperationFailed`, `ReactionHandlerMessageSendFailed`, `LocalizationLoading`, `LocalizationLoaded`, `LocalizationReadFailed`, `LocalizationWriteFailed`, `ValidationUserIsBot`, `ValidationUserNotInVoiceChannel`, `DiscordClientReady`, `DiscordClientGuildAvailable`, `DiscordClientEventFailed`, `PlayCommandStartUrl`, `PlayCommandStartQuery`, `QueueIsEmpty`, `LavalinkNodeConnectedSuccessfully`, `LavalinkConnectionFailed`, `ThereIsNoTrackCurrentlyPlaying`, `ThereIsNoTrackCurrentlyPaused`, `NowPlaying`, `LavalinkOperationFailed`, `ResponseSendFailed`, `MessageSendFailed`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
logger.CommandExecuted(commandName);
```

---

### LoggingScopes

**File:** `DC bot/Logging/LoggingScopes.cs`

**Purpose:** Attaches command and guild identifiers to the logging scope of an operation.

**Methods:**

- `BeginCommandScope` — Creates a logging scope containing command and Discord context identifiers.

```csharp
public static IDisposable? BeginCommandScope(
    this ILogger logger,
    string commandName,
    ulong? userId,
    ulong? channelId,
    ulong? guildId);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = logger.BeginCommandScope(commandName, userId, channelId, guildId);
```

---

## Related Components

- `DC bot/Logging/EventIdTable.md` — Event catalog and visibility rules derived from the current logging sources.
- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
