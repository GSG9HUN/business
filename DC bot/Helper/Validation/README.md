# Validation

Carries user, player, and connection validation outcomes without requiring exceptions for ordinary rejection.

## Overview

- ValidationService and command helpers return a success flag, an error key, and any validated member, channel, or player needed downstream.
- Consumers must inspect validity before using optional context. These values report a check at one moment, not permanent ownership of a player session.

## Validation Result Types

### ConnectionValidationResult

**File:** `DC bot/Helper/Validation/ConnectionValidationResult.cs`

**Purpose:** Carries outcome and response data for connection validation operations.

**Properties:**

```csharp
// Whether validation succeeded.
public bool IsValid { get; }
public string ErrorKey { get; }
public ILavalinkPlayer? Connection { get; }
```

**Usage:**

```csharp
// The localization key explaining a failed validation.
var result = await validationService.ValidateConnectionAsync(player, cancellationToken);

if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey, cancellationToken);
    return;
}
```

**Validation Checks:**

- The SDK connected flag can confirm readiness immediately.
- When the SDK flag lags, ValidationService can confirm matching node session, Discord voice session, and channel through a bounded REST probe.
- Remote evidence is rejected if the captured local session or channel changed while the request was in flight.

---

### PlayerValidationResult

**File:** `DC bot/Helper/Validation/PlayerValidationResult.cs`

**Purpose:** Carries outcome and response data for player validation operations.

**Properties:**

```csharp
// Whether validation succeeded.
public bool IsValid { get; }
public string ErrorKey { get; }
public ILavalinkPlayer? Player { get; }
```

**Usage:**

```csharp
// The localization key explaining a failed validation.
var result = await validationService.ValidatePlayerAsync(audioService, guildId, cancellationToken);

if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey, cancellationToken);
    return;
}

var player = result.Player;
```

**Validation Checks:**

- A Lavalink player must exist for the guild.
- A player lookup result is not a permanent reservation of that player generation.

---

### UserValidationResult

**File:** `DC bot/Helper/Validation/UserValidationResult.cs`

**Purpose:** Carries outcome and response data for user validation operations.

**Properties:**

```csharp
// Whether validation succeeded.
public bool IsValid { get; }
public string ErrorKey { get; }
public IDiscordMember? Member { get; }
```

**Usage:**

```csharp
// The localization key explaining a failed validation.
var result = await validationService.ValidateUserAsync(message, cancellationToken);

if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey, cancellationToken);
    return;
}

var member = result.Member;
```

**Validation Checks:**

- User validation rejects bot users unless test mode permits them.
- The member must be in a voice channel; unsuccessful results carry a localization error key.

---

## Related Components

- `DC bot/Helper/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
