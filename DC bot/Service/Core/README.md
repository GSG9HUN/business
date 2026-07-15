# Core Services

This folder contains core application services for command handling and validation.

## Files

### CommandHandlerService.cs

**Purpose:** Route Discord messages to appropriate command handlers.

**Implements:** No interface (event handler service)

**Key Methods:**

- `HandleEventAsync()` - Receives DSharpPlus `MessageCreatedEventArgs`
- `RegisterHandler()` - Enables text command processing after the service graph is built
- `UnregisterHandler()` - Disables processing in tests/internal cleanup

**Behavior:**

1. Receives message-created events wired by `Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`
2. Ignores messages until `RegisterHandler()` has enabled processing
3. Ignores bot authors unless the service was created in test mode
4. Parses the configured prefix (e.g., `!play`)
5. Extracts command name and argument text
6. Finds the matching `ICommand` through `ICommandRegistry`
7. Wraps the DSharpPlus message with explicit guild context
8. Executes the command and logs the result
9. Sends a guild-localized unknown-command response when no command matches

**Initialization:**

```csharp
BotHandlerRegistrar.RegisterHandlers(services);
```

Event routing itself is configured through DSharpPlus 5 builder APIs in `Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`.

---

### ValidationService.cs

**Purpose:** Validate user, player, and connection state.

**Implements:**

- `IValidationService` - Player and connection validation
- `IUserValidationService` - User validation

**Methods:**

#### IValidationService

- `ValidatePlayerAsync()` - Check if Lavalink player exists for guild
- `ValidateConnectionAsync()` - Check if player connection is established

#### IUserValidationService

- `ValidateUserAsync()` - Validate user (not bot, in voice channel)
- `IsBotUser()` - Check if message author is a bot

**Usage:**

```csharp
// Validate user
var userResult = await validationService.ValidateUserAsync(message);
if (!userResult.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, userResult.ErrorKey);
    return;
}

// Validate player
var playerResult = await validationService.ValidatePlayerAsync(audioService, guildId);
if (!playerResult.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, playerResult.ErrorKey);
    return;
}
```

**Validation Checks:**

User validation:

- User is not a bot
- User is in a voice channel
- Bot users are ignored unless test mode is enabled

Player validation:

- Player exists for guild
- `ValidatePlayerAsync()` logs Lavalink connection state, player state, voice channel ID, and current track identifier when a player is found
- `ValidateConnectionAsync()` requires `connection.ConnectionState.IsConnected`; disconnected or destroyed players fail with `BotIsNotConnectedError` even if they still have a voice channel ID

---

### CommandValidationService.cs

**Purpose:** Command-specific validation logic.

**Details:** Used by commands for custom validation beyond standard user/player checks.

Argument extraction treats missing and whitespace-only arguments as invalid. This keeps commands such as `!play   `
from receiving an empty payload.

Playlist commands that need two values, such as `!savePlaylist <name> <url>`, `!addSong <name> <url>`,
`!removeSong <name> <trackNumber>`, and `!renamePlaylist <currentName> <newName>`, use
`TryParseSavePlaylistArguments` to validate and split the payload.

---

### CommandRegistry.cs

**Purpose:** Materialize registered `ICommand` implementations once and expose name-based lookup.

**Behavior:**

- Injected into `CommandHandlerService` and `HelpCommand` instead of resolving commands through `IServiceProvider`.
- Builds a stable command list and dictionary from the DI-provided `IEnumerable<ICommand>`.
- Duplicate command names fail during lookup initialization, so ambiguous command routing is caught early.

---

## Related Components

- **Interface/Core/** - Service contracts
- **Commands/** - Use these services
- **Helper/Validation/** - Validation result types
- **Service/LocalizationService.cs** - Localization lookup

