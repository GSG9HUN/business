# Helper Validation

This folder contains validation result models used by validation services.

## Overview

Result models represent validation outcomes **without using exceptions**. These are returned by `ValidationService` and
consumed by commands.

## Files

### ConnectionValidationResult.cs

**Definition:**

```csharp
public class ConnectionValidationResult(bool isValid, string errorKey, ILavalinkPlayer? connection)
{
    public bool IsValid { get; }
    public string ErrorKey { get; }
    public ILavalinkPlayer? Connection { get; }
}
```

**Purpose:** Represents the result of validating a Lavalink player connection.

**Properties:**

- `IsValid` - Whether the connection is valid
- `ErrorKey` - Localization key for error message (if invalid)
- `Connection` - The validated connection (if valid)

**Usage:**

```csharp
var result = await validationService.ValidateConnectionAsync(player);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

// Use result.Connection
```

**Used By:**

- `Service/Core/ValidationService.cs` - Returns this result
- Music commands - Consume this result

---

### PlayerValidationResult.cs

**Definition:**

```csharp
public class PlayerValidationResult(bool isValid, string errorKey, ILavalinkPlayer? player)
{
    public bool IsValid { get; }
    public string ErrorKey { get; }
    public ILavalinkPlayer? Player { get; }
}
```

**Purpose:** Represents the result of validating a Lavalink player exists.

**Properties:**

- `IsValid` - Whether the player is valid
- `ErrorKey` - Localization key for error message (if invalid)
- `Player` - The validated player (if valid)

**Usage:**

```csharp
var result = await validationService.ValidatePlayerAsync(audioService, guildId);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

var player = result.Player;
await player.PauseAsync();
```

**Used By:**

- `Service/Core/ValidationService.cs` - Returns this result
- Music commands - Consume this result

---

### UserValidationResult.cs

**Definition:**

```csharp
public class UserValidationResult(bool isValid, string errorKey, IDiscordMember? member = null)
{
    public bool IsValid { get; }
    public string ErrorKey { get; }
    public IDiscordMember? Member { get; }
}
```

**Purpose:** Represents the result of validating a Discord user for command execution.

**Properties:**

- `IsValid` - Whether the user is valid
- `ErrorKey` - Localization key for error message (if invalid)
- `Member` - The Discord member (if valid)

**Usage:**

```csharp
var result = await validationService.ValidateUserAsync(message);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

var voiceChannel = result.Member?.VoiceState?.Channel;
```

**Validation Checks:**

- User is not a bot
- User is in a voice channel
- User is in the same voice channel as bot (if bot is connected)

**Used By:**

- `Service/Core/ValidationService.cs` - Returns this result
- All commands - Validate user before execution

---

## Why Result Objects Instead of Exceptions?

### Advantages:

- **Expected outcomes** - Validation failures are expected, not exceptional
- **Performance** - No exception overhead
- **Control flow** - Explicit handling without try-catch
- **Type safety** - Compile-time checking

### Comparison:

**With Exceptions (❌ Not used):**

```csharp
try
{
    await validationService.ValidateUserAsync(message);
    // Continue...
}
catch (ValidationException ex)
{
    await responseBuilder.SendErrorAsync(message, ex.Message);
}
```

**With Result Objects (✅ Current approach):**

```csharp
var result = await validationService.ValidateUserAsync(message);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}
// Continue...
```

---

## Common Pattern in Commands

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
{
    logger.CommandInvoked(Name);
    
    // Validate user
    var validationResult = await commandHelper.TryValidateUserAsync(
        userValidation, responseBuilder, message);
    if (validationResult is null) return; // Already sent error to user
    
    // Command logic with validated member
    var voiceChannel = validationResult.Member?.VoiceState?.Channel;
    await lavaLinkService.DoSomethingAsync(voiceChannel);
    
    logger.CommandExecuted(Name);
}
```

---

## Related Components

- **Service/Core/ValidationService.cs** - Creates these result objects
- **Interface/Core/IValidationService.cs** - Validation service contracts
- **Interface/Core/IUserValidationService.cs** - User validation contract
- **Commands/** - Consume these result objects
- **Exceptions/Validation/ValidationException.cs** - Not used (result objects preferred)

