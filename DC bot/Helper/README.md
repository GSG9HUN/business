# Helper

Contains small adapters and utilities used by command and presentation boundaries.

## Overview

- DiscordTextSanitizer prepares text for Discord output; factories wrap SDK messages and validation models carry normalized outcomes.
- Keep helpers focused. A helper should not become an alternate playback coordinator or bypass the injected service graph.

## Subfolders

### Factory/

Creates the message wrapper used by command dispatch.

**Files:**

- `DiscordMessageWrapperFactory.cs`

**Purpose:** DiscordMessageWrapperFactory captures the SDK message together with resolved guild context and exposes IDiscordMessage.

---

### Validation/

Carries user, player, and connection validation outcomes without requiring exceptions for ordinary rejection.

**Files:**

- `ConnectionValidationResult.cs`
- `PlayerValidationResult.cs`
- `UserValidationResult.cs`

**Purpose:** ValidationService and command helpers return a success flag, an error key, and any validated member, channel, or player needed downstream.

---

## Classes

### DiscordTextSanitizer

**File:** `DC bot/Helper/DiscordTextSanitizer.cs`

**Purpose:** Escapes user-controlled text that could trigger unintended Discord mentions.

**Methods:**

- `EscapeMentions` — Escapes mention syntax in user-controlled text before Discord presentation.

```csharp
public static string EscapeMentions(string? text);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = DiscordTextSanitizer.EscapeMentions(text);
```

---

## Validation Result Types

### ConnectionValidationResult

**File:** `DC bot/Helper/Validation/ConnectionValidationResult.cs`

**Purpose:** Carries outcome and response data for connection validation operations.

**Properties:**

```csharp
public bool IsValid { get; }
public string ErrorKey { get; }
public ILavalinkPlayer? Connection { get; }
```

**Usage:**

```csharp
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
public bool IsValid { get; }
public string ErrorKey { get; }
public ILavalinkPlayer? Player { get; }
```

**Usage:**

```csharp
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
public bool IsValid { get; }
public string ErrorKey { get; }
public IDiscordMember? Member { get; }
```

**Usage:**

```csharp
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

## Factory Classes

### DiscordMessageWrapperFactory

**File:** `DC bot/Helper/Factory/DiscordMessageWrapperFactory.cs`

**Purpose:** Creates message adapters at the DSharpPlus command boundary.

**Implements / inherits:** `IDiscordMessageFactory`.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public static IDiscordMessage Create(
    DiscordMessage message,
    DiscordChannel channel,
    DiscordUser author,
    ILogger<DiscordMessageWrapper>? logger = null,
    DiscordGuild? guild = null);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var wrappedMessage = DiscordMessageWrapperFactory.Create(
    discordMessage,
    channel,
    author,
    logger,
    guild);
```

**Benefits:**

- Centralizes message wrapper construction.
- Preserves explicit guild context when the channel payload does not supply it.
- Supports the injected IDiscordMessageFactory boundary while retaining the static helper.

---

## Related Components

- `DC bot/Service/Core/ValidationService.cs` — Creates user, player, and connection validation results.
- `DC bot/Wrapper/Discord/` — Implements the Discord wrappers created by the factory.
- `DC bot/Interface/Discord/` — Defines the wrapper contracts consumed by commands.
- `DC bot/Commands/` — Consumes validation results before invoking use cases.
- `DC bot/Helper/Factory/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Helper/Validation/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
