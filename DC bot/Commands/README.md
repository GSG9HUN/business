# Commands

This folder contains all text-based command implementations for the Discord bot.

## What's here?

This directory holds the concrete command classes that handle user interactions through text messages (prefixed with `!`). Each command implements the `ICommand` interface and follows a consistent pattern for validation, execution, and error handling.

## Command Structure

All commands follow this pattern:
- Implement `ICommand` interface (`Name`, `Description`, `ExecuteAsync`)
- Use dependency injection for services
- Leverage `CommandValidationHelper` for common validation
- Use structured logging via `LogExtensions`
- Handle localization through `ILocalizationService`

## Available Commands

### Music Playback
- **PlayCommand.cs** - Play music from URL or search query
- **PauseCommand.cs** - Pause current playback
- **ResumeCommand.cs** - Resume paused playback
- **SkipCommand.cs** - Skip to next track in queue
- **JoinCommand.cs** - Join user's voice channel
- **LeaveCommand.cs** - Leave voice channel and cleanup

### Queue Management
- **ViewQueueCommand.cs** - Display current queue
- **ClearCommand.cs** - Clear the music queue
- **ShuffleCommand.cs** - Shuffle queue order
- **RepeatCommand.cs** - Toggle repeat mode for current track
- **RepeatListCommand.cs** - Toggle repeat mode for entire queue

### Utility
- **PingCommand.cs** - Check bot responsiveness
- **HelpCommand.cs** - Display available commands
- **LanguageCommand.cs** - Change bot language
- **TagCommand.cs** - Manage custom tags

## Slash Commands

The **SlashCommands/** subfolder contains modern Discord slash command implementations that provide a richer user experience with:
- Auto-complete
- Type validation
- Better UI integration

See: `SlashCommands/README.md`

## Usage Example

```csharp
public class PlayCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILogger<PlayCommand> logger) : ICommand
{
    public string Name => "play";
    public string Description => "Play music";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        
        // Validate user (not a bot, in voice channel)
        var validationResult = await CommandValidationHelper
            .TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        // Extract arguments
        var query = await CommandValidationHelper
            .TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (query is null) return;

        // Execute command logic
        await lavaLinkService.PlayAsyncQuery(voiceChannel, query, message, searchMode);
        
        logger.CommandExecuted(Name);
    }
}
```

## Validation Flow

1. **User Validation** - Ensure user is not a bot and is in a voice channel
2. **Argument Extraction** - Parse command arguments from message
3. **Service Layer** - Delegate to appropriate service (LavaLinkService, etc.)
4. **Response** - Send feedback to user via `IResponseBuilder`
5. **Logging** - Log command invocation and completion

## Error Handling

Commands use a consistent error handling pattern:
- Validation errors → Send localized error message via `ResponseBuilder`
- Service exceptions → Caught in service layer, logged with context
- Unexpected errors → Logged at command level with full context

## Dependencies

Common dependencies across commands:
- `ILavaLinkService` - Music playback operations
- `IUserValidationService` - User validation
- `IResponseBuilder` - Send messages to Discord
- `ILocalizationService` - Multi-language support
- `ILogger<T>` - Structured logging
- `IMusicQueueService` - Queue management

## Testing

Commands can be tested by:
1. Mocking the service dependencies
2. Creating test `IDiscordMessage` implementations
3. Verifying service method calls
4. Checking response messages

See: `DC bot tests/UnitTests/CommandTests/`

## Best Practices

- ✅ Use `CommandValidationHelper` for common validation
- ✅ Log command invocation and execution
- ✅ Return early on validation failures
- ✅ Use localization keys for all user-facing text
- ✅ Leverage dependency injection
- ✅ Keep commands thin - delegate to services
- ❌ Don't put business logic in commands
- ❌ Don't directly reference DSharpPlus types (use wrappers)
- ❌ Don't hardcode strings (use `LocalizationKeys`)

## Related

- **Interface/ICommand.cs** - Command contract
- **Helper/CommandValidationHelper.cs** - Shared validation utilities
- **Service/** - Business logic layer
- **Logging/LogExtensions.cs** - Command logging extensions

