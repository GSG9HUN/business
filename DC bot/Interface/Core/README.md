# Core Interfaces

This folder contains core application interface contracts.

## Files

### ICommandHelper.cs

**Purpose:** Command validation and argument extraction helpers.

```csharp
public interface ICommandHelper
{
    Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation, 
        IResponseBuilder responseBuilder, 
        IDiscordMessage message);
    
    Task<string?> TryGetArgumentAsync(
        IDiscordMessage message, 
        IResponseBuilder responseBuilder, 
        ILogger logger, 
        string commandName);
}
```

**Methods:**
- `TryValidateUserAsync()` - Validates user and sends error if invalid
- `TryGetArgumentAsync()` - Extracts command arguments from message

**Implementation:** `Service/Core/CommandHelperService.cs` (if exists) or used in commands directly

---

### IValidationService.cs

**Purpose:** Player and connection validation.

```csharp
public interface IValidationService
{
    Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId);
    Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection);
}
```

**Methods:**
- `ValidatePlayerAsync()` - Check if player exists for guild
- `ValidateConnectionAsync()` - Check if connection is established

**Implementation:** `Service/Core/ValidationService.cs`

---

### IUserValidationService.cs

**Purpose:** User validation for command execution.

```csharp
public interface IUserValidationService
{
    Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message);
    bool IsBotUser(IDiscordMessage message);
}
```

**Methods:**
- `ValidateUserAsync()` - Validate user is in voice channel, not a bot, etc.
- `IsBotUser()` - Check if message author is a bot

**Implementation:** `Service/Core/ValidationService.cs`

---

## Related Components

- **Service/Core/ValidationService.cs** - Implements these interfaces
- **Helper/Validation/** - Validation result types
- **Commands/** - Use these interfaces for validation

