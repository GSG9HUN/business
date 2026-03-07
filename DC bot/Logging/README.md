# Logging

This folder contains structured logging extensions and configuration.

## Files

### LogExtensions.cs

**Purpose:** Structured logging methods for application events.

**Format:**
```csharp
[LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "...")]
public static partial void MethodName(this ILogger logger, ...parameters);
```

**Usage:**
```csharp
logger.CommandInvoked("play");           // EventId 1001
logger.CommandExecuted("play");          // EventId 1002
logger.LavalinkNodeConnectedSuccessfully();  // Lavalink event
logger.ValidationUserIsBot();            // Validation event
logger.ValidationUserNotInVoiceChannel(); // Validation event
```

**Benefits:**
- Type-safe logging
- Structured parameters
- Consistent format
- Unique event IDs
- No string concatenation

---

### LoggingScopes.cs

**Purpose:** Logging scope helpers for context tracking.

**Usage:**
```csharp
using (logger.BeginScope(new { GuildId = guildId, UserId = userId }))
{
    // Logs in this scope automatically include GuildId and UserId
    logger.CommandInvoked("play");
}
```

---

### EventIdTable.md

**Purpose:** Documentation of all event IDs used in logging.

**Format:**
```
1001 - CommandInvoked
1002 - CommandExecuted
1003 - CommandMissingArgument
1004 - CommandExecutionFailed
1101 - CommandHandlerAlreadyRegistered
...
```

**Benefits:**
- Single source of truth for event IDs
- Prevents ID collisions
- Aids debugging and monitoring

---

## Log Levels

- **Debug** - Detailed execution flow (command invocation)
- **Information** - Normal operation (handler registered, message sent)
- **Warning** - Unexpected but handled (rate limit, retry)
- **Error** - Operation failed (command execution, connection)
- **Critical** - Application may not recover (startup failure)

## Related Components

- **Service/** - Uses logging extensions
- **Commands/** - Log command execution
- **Wrapper/** - Log Discord operations

