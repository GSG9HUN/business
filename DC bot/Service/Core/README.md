# Core Services

This folder contains core application services for command handling and validation.

## Files

### CommandHandlerService.cs

**Purpose:** Route Discord messages to appropriate command handlers.

**Implements:** No interface (event handler service)

**Key Methods:**

- `RegisterHandler()` - Register message event handler
- `HandleCommandAsync()` - Process Discord messages

**Behavior:**

1. Listens to Discord MessageCreated events
2. Parses message prefix (e.g., `!play`)
3. Extracts command name
4. Finds matching `ICommand` from DI container
5. Executes command
6. Logs result

**Initialization:**

```csharp
var commandHandler = new CommandHandlerService(services, logger, localization, botSettings);
commandHandler.RegisterHandler(discordClient);
```

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
- User is in same voice channel as bot (if bot connected)

Player validation:

- Player exists for guild
- Connection is established

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

