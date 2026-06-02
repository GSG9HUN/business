# Helper

This folder contains utility classes and helper methods.

## Overview

Reusable helper classes that:

- Reduce code duplication
- Provide validation result types
- Offer factory methods for wrappers

## Subfolders

### Validation/

Contains validation result models.

**Files:**

- `ConnectionValidationResult.cs`
- `PlayerValidationResult.cs`
- `UserValidationResult.cs`

**Purpose:** Result objects for validation operations (alternative to exceptions).

---

### Factory/

Contains factory classes for object creation.

**Files:**

- `DiscordMessageWrapperFactory.cs`

**Purpose:** Create Discord wrapper instances from DSharpPlus objects.

---

## Validation Result Types

### UserValidationResult

**Properties:**

```csharp
public bool IsValid { get; }
public string ErrorKey { get; }
public IDiscordMember? Member { get; }
```

**Usage:**

```csharp
var result = await validationService.ValidateUserAsync(message);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

var member = result.Member;
```

**Validation Checks:**

- User is not a bot
- User is in a voice channel
- Bot users are invalid unless `ValidationService` is constructed in test mode

---

### PlayerValidationResult

**Properties:**

```csharp
public bool IsValid { get; }
public string ErrorKey { get; }
public ILavalinkPlayer? Player { get; }
```

**Usage:**

```csharp
var result = await validationService.ValidatePlayerAsync(audioService, guildId);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

var player = result.Player;
```

**Validation Checks:**

- Player exists for guild
- Player is connected to voice channel

---

### ConnectionValidationResult

**Properties:**

```csharp
public bool IsValid { get; }
public string ErrorKey { get; }
public ILavalinkPlayer? Connection { get; }
```

**Usage:**

```csharp
var result = await validationService.ValidateConnectionAsync(player);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}
```

**Validation Checks:**

- Connection state reports connected, or the player has a non-zero voice channel and is not destroyed

---

## Factory Classes

### DiscordMessageWrapperFactory

**Purpose:** Create `IDiscordMessage` wrapper from DSharpPlus `DiscordMessage`.

**Method:**

```csharp
public static IDiscordMessage Create(
    DiscordMessage message, 
    DiscordChannel channel, 
    DiscordUser author, 
    ILogger<DiscordMessageWrapper>? logger = null,
    DiscordGuild? guild = null)
```

**Usage:**

```csharp
var wrappedMessage = DiscordMessageWrapperFactory.Create(
    discordMessage, 
    channel, 
    author, 
    logger,
    guild
);
```

**Benefits:**

- Centralizes wrapper creation logic
- Ensures consistent wrapper initialization
- Preserves explicit guild context for event payloads where `DiscordChannel.Guild` is not populated
- Simplifies testing with mock objects

---

## Related Components

- **Service/Core/ValidationService.cs** - Creates validation result objects
- **Wrapper/** - Discord wrapper implementations
- **Interface/Discord/** - Discord interface contracts
- **Commands/** - Consume validation results

