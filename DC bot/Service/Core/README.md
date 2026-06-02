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

1. Receives message-created events wired in `Startup/BotServiceProviderFactory.cs`
2. Ignores messages until `RegisterHandler()` has enabled processing
3. Ignores bot authors unless the service was created in test mode
4. Parses the configured prefix (e.g., `!play`)
5. Extracts command name and argument text
6. Finds the matching `ICommand` from DI
7. Wraps the DSharpPlus message with explicit guild context
8. Executes the command and logs the result
9. Sends a guild-localized unknown-command response when no command matches

**Initialization:**

```csharp
BotHandlerRegistrar.RegisterHandlers(services);
```

Event routing itself is configured through DSharpPlus 5 builder APIs in `Startup/BotServiceProviderFactory.cs`.

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
- Connection is considered valid when Lavalink reports an active connection, or when the player is not destroyed and still has a voice channel ID

---

### CommandValidationService.cs

**Purpose:** Command-specific validation logic.

**Details:** Used by commands for custom validation beyond standard user/player checks.

---

## Related Components

- **Interface/Core/** - Service contracts
- **Commands/** - Use these services
- **Helper/Validation/** - Validation result types
- **Service/LocalizationService.cs** - Localization lookup

