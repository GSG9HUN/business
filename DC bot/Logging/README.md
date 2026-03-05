# Logging Extensions

This folder contains structured logging (ILogger) extensions and scope helpers for the application.

## What's here?

- **LogExtensions.cs** - `LoggerMessage`-based source-generated logging extension methods
- **LoggingScopes.cs** - Unified scope helper for consistent structured logging (e.g., during command execution)

## Event ID Reference

See: `EventIdTable.md` for complete event ID mapping

## Usage Example

```csharp
using DC_bot.Logging;

// Simple logging
logger.CommandInvoked(commandName);
logger.CommandExecuted(commandName);

// Structured logging with scope
using var scope = logger.BeginCommandScope(commandName, userId, channelId, guildId);
logger.LogInformation("Processing command");
```

## Extension Methods

### Command Logging
```csharp
// Log command invocation
logger.CommandInvoked(string commandName);

// Log successful execution
logger.CommandExecuted(string commandName);

// Log missing arguments
logger.CommandMissingArgument(string commandName);

// Log execution failure
logger.CommandExecutionFailed(string commandName, Exception ex);
```

### Lavalink Logging
```csharp
logger.LavalinkNodeConnectedSuccessfully();
logger.LavalinkConnectionFailed(Exception ex, string message);
logger.LavalinkOperationFailed(Exception ex, string operation);
logger.PlaybackFinishedEventRegistered();
```

### Validation Logging
```csharp
logger.ValidationLavalinkNotConnected();
logger.ValidationBotNotConnected();
logger.ValidationUserIsBot();
logger.ValidationUserNotInVoiceChannel();
```

### Queue Logging
```csharp
logger.AddedToQueue();
logger.AddedToQueueWithTrackDetails(string author, string title);
logger.QueueIsEmpty();
logger.MusicQueueSaveFailed(string filePath, Exception ex);
logger.MusicQueueLoadFailed(string filePath, Exception ex);
```

## Logging Levels

Use appropriate levels for different scenarios:

- **Debug** - `CommandInvoked`, `CommandExecuted`
  - Low-level operation tracking
  - Command invocations and completions
  - Should be disabled in production

- **Information** - `ValidationLavalinkNotConnected`, `AddedToQueue`, `PlaybackFinished`
  - Important state changes
  - Significant events
  - Safe to keep in production

- **Warning** - `CommandHandlerUnregisterNotRegistered`, `ReactionHandlerUnregisterNotRegistered`
  - Unexpected but recoverable conditions
  - Suggest a problem but not a failure
  - Requires investigation

- **Error** - `CommandExecutionFailed`, `LavalinkConnectionFailed`, `ResponseSendFailed`
  - Serious problems
  - Operation failures
  - Exceptions with context
  - Always logged with exception details

## Scope Helpers

### Command Scope
```csharp
using var scope = logger.BeginCommandScope(
    commandName: "play",
    userId: 123456,
    channelId: 789012,
    guildId: 345678);

// All logs within this scope include the context
logger.LogInformation("Executing command");
// Output: [play] [user:123456] [channel:789012] [guild:345678] Executing command
```

**Includes in scope:**
- Command name
- User ID
- Channel ID
- Guild ID

**Use when:** Executing commands or guild-specific operations

## Source-Generated Logging

The extension methods use `LoggerMessage.Define()` for optimal performance:

```csharp
[LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Debug,
    Message = "Command invoked: {CommandName}")]
private static partial void CommandInvokedCore(
    ILogger logger,
    string commandName);

public static void CommandInvoked(
    this ILogger logger,
    string commandName) =>
    CommandInvokedCore(logger, commandName);
```

**Benefits:**
- ✅ Zero-allocation logging
- ✅ AOT-friendly
- ✅ Compile-time checking
- ✅ Performance optimized
- ✅ Type-safe

## Guidelines

### ✅ DO's
- ✅ Include relevant context in log messages
- ✅ Use appropriate log levels
- ✅ Use scope helpers for related operations
- ✅ Log exceptions with full context
- ✅ Use structured logging (named properties)

### ❌ DON'Ts
- ❌ Don't log sensitive information (tokens, passwords)
- ❌ Don't use string concatenation (use parameters)
- ❌ Don't log on every iteration (causes spam)
- ❌ Don't forget to dispose scopes (use `using`)
- ❌ Don't mix logging levels in related operations

## Example: Command Execution Flow

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
{
    logger.CommandInvoked(Name); // Debug
    
    using var scope = logger.BeginCommandScope(
        Name, 
        message.Author.Id, 
        message.Channel.Id, 
        message.Channel.Guild.Id);

    try
    {
        var result = await userValidation.ValidateUserAsync(message);
        if (!result.IsValid)
        {
            logger.LogWarning("User validation failed: {ErrorKey}", result.ErrorKey);
            return;
        }

        await service.ExecuteAsync(message);
        logger.CommandExecuted(Name); // Debug
    }
    catch (Exception ex)
    {
        logger.CommandExecutionFailed(Name, ex); // Error
        throw;
    }
}

// Output:
// DEBUG: Command invoked: play
// INFO: [play] [user:123456] [channel:789012] [guild:345678] (scope active)
// DEBUG: Command executed: play
```

## Viewing Logs

### Console Output (Development)
```
info: DC_bot.Commands.PlayCommand[1602]
      Starting playing a music through search result.
dbug: DC_bot.Commands.PlayCommand[1001]
      Command invoked: play
```

### Structured Logging (Production)
Logs include:
- Timestamp
- Log level
- Event ID (for filtering)
- Category (namespace)
- Message
- Structured properties
- Exception (if applicable)

## Performance Implications

- **Source-generated logging** - Zero allocations
- **Scope allocation** - Minimal overhead
- **Structured parameters** - No string formatting overhead
- **Debug logs in production** - Negligible impact (filtered out)

## Related

- **EventIdTable.md** - Complete event ID reference
- **Service/** - Consumers of logging extensions
- **Commands/** - Primary logging users
- **Program.cs** - Logging configuration
