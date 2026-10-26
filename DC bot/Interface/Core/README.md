# Core

Defines command lookup, argument helpers, and user/player validation boundaries.

## Overview

- Command routing and command implementations depend on these abstractions; implementations live in Service/Core.
- Keep registry lookup, argument parsing, and readiness validation as separate responsibilities rather than expanding ICommand itself.

## Interfaces

### ICommandHelper

**File:** `DC bot/Interface/Core/ICommandHelper.cs`

**Purpose:** Defines the command helper boundary so callers are independent of its concrete implementation.

**Methods:**

- `TryGetArgumentAsync` — Resolves a required argument and presents usage guidance when absent.

```csharp
Task<string?> TryGetArgumentAsync(
    IDiscordMessage message,
    IResponseBuilder responseBuilder,
    ILogger logger,
    string commandName,
    CancellationToken cancellationToken = default);
```

- `TryParseSavePlaylistArguments` — Parses a playlist name and source argument while preserving quoted values.

```csharp
Task<(
    IDiscordMessage message,
    IResponseBuilder responseBuilder,
    ILogger logger,
    string commandName,
    CancellationToken cancellationToken = default);
```

- `TryValidateUserAsync` — Runs user validation and sends a localized failure response when validation fails.

```csharp
Task<UserValidationResult?> TryValidateUserAsync(
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    IDiscordMessage message,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, commandName, cancellationToken);
```

---

### ICommandRegistry

**File:** `DC bot/Interface/Core/ICommandRegistry.cs`

**Purpose:** Defines the command registry boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The registered text commands available for dispatch.
IReadOnlyCollection<ICommand> Commands { get; }
```

**Methods:**

- `TryGetCommand` — Looks up a command by name without rebuilding the lazy registry snapshot.

```csharp
bool TryGetCommand(string commandName, out ICommand command);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = commandRegistry.TryGetCommand(commandName, out var command);
```

---

### IUserValidationService

**File:** `DC bot/Interface/Core/IUserValidationService.cs`

**Purpose:** Defines the user validation service boundary so callers are independent of its concrete implementation.

**Methods:**

- `ValidateUserAsync` — Checks that the requesting Discord user may perform voice-bound music operations.

```csharp
public Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message, CancellationToken cancellationToken = default);
```

- `IsBotUser` — Checks whether an event user is the currently authenticated bot.

```csharp
public bool IsBotUser(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await userValidationService.ValidateUserAsync(message, cancellationToken);
```

---

### IValidationService

**File:** `DC bot/Interface/Core/IValidationService.cs`

**Purpose:** Defines the validation service boundary so callers are independent of its concrete implementation.

**Methods:**

- `ValidateConnectionAsync` — Validates Lavalink readiness using available player and remote voice-state evidence.

```csharp
public Task<ConnectionValidationResult> ValidateConnectionAsync(
    ILavalinkPlayer connection,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await validationService.ValidateConnectionAsync(connection, cancellationToken);
```

---

## Related Components

- `DC bot/Interface/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Helper/Validation/` — Carries user, player, and connection validation outcomes without requiring exceptions for ordinary rejection.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Presentation/` — Defines localized success, warning, error, and validation responses.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
