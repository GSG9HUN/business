# Commands

This folder contains command implementations for the Discord bot.

## Overview

Commands handle user input from Discord messages and delegate business logic to services. The bot supports two command
types:

1. **Text Commands** - Prefix-based commands (`!play`, `!pause`)
2. **Slash Commands** - Discord interactions (`/play`, `/ping`)

## Folder Structure

### Music/

Music playback control commands.

**Commands:**

- `PlayCommand` - Play music from URL or search query
- `PauseCommand` - Pause current track
- `ResumeCommand` - Resume paused track
- `SkipCommand` - Skip to next track
- `JoinCommand` - Join user's voice channel
- `LeaveCommand` - Disconnect from voice channel

---

### Queue/

Queue management commands.

**Commands:**

- `ViewQueueCommand` - Display current queue
- `ShuffleCommand` - Randomize queue order
- `RepeatCommand` - Toggle single-track repeat
- `RepeatListCommand` - Toggle queue repeat
- `ClearCommand` - Clear all queued tracks

---

### SlashCommands/

Slash command implementations.

**Commands:**

- `PlaySlashCommand` - `/play`
- `PingSlashCommand` - `/ping`
- `HelpSlashCommand` - `/help`
- `TagSlashCommand` - `/tag`

---

### Utility/

General-purpose bot commands.

**Commands:**

- `HelpCommand` - List all commands
- `PingCommand` - Check bot latency
- `LanguageCommand` - Change guild language
- `TagCommand` - Manage custom tags

---

## ICommand Interface

Text commands implement `ICommand`:

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task ExecuteAsync(IDiscordMessage message);
}
```

## Command Flow

```
Discord Message → CommandHandlerService
                   ↓
              Parse Prefix (!play)
                   ↓
         Find ICommand (PlayCommand)
                   ↓
    command.ExecuteAsync(message)
                   ↓
       1. Validate user
       2. Extract arguments
       3. Call service
       4. Send response
```

## Common Pattern

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
{
    logger.CommandInvoked(Name);
    
    var validationResult = await commandHelper.TryValidateUserAsync(
        userValidation, responseBuilder, message);
    if (validationResult is null) return;
    
    // Command logic
    await service.DoSomethingAsync();
    
    logger.CommandExecuted(Name);
}
```

## Dependencies

Commands typically inject:

- `ILavaLinkService` / `IMusicQueueService` - Business logic
- `IUserValidationService` - User validation
- `IResponseBuilder` - Message sending
- `ILocalizationService` - Multi-language support
- `ICommandHelper` - Validation helpers
- `ILogger<T>` - Logging

## Registration

Commands are registered in `Program.cs`:

```csharp
services.AddSingleton<ICommand, PlayCommand>();
services.AddSingleton<ICommand, PauseCommand>();
// ... more commands
```

## Related Components

- **Interface/ICommand.cs** - Text command contract
- **Service/CommandHandlerService.cs** - Command routing
- **Service/ValidationService.cs** - User validation
- **Helper/CommandHelper.cs** - Command utilities

