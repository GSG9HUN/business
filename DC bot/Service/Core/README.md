# Core

Handles text command routing, registry snapshots, argument parsing, and user/player readiness validation.

## Overview

- CommandHandlerService dispatches registered ICommand values. CommandRegistry materializes lookup state; CommandValidationService parses command payloads. ValidationService checks voice context and player readiness.
- If the SDK reports disconnected, readiness can use REST evidence tied to unchanged node and Discord voice sessions. A voice channel ID alone is not sufficient proof of a valid connection.

## Classes

### CommandHandlerService

**File:** `DC bot/Service/Core/CommandHandlerService.cs`

**Purpose:** Dispatches prefixed Discord messages through the registry and contains command execution failures.

**Implements / inherits:** `IEventHandler<MessageCreatedEventArgs>`.

**Methods:**

- `HandleEventAsync` — Dispatches a message-created event only while this command handler is registered.

```csharp
public Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs);
```

- `RegisterHandler` — Registers runtime event handlers while preventing duplicate subscriptions.

```csharp
public void RegisterHandler(DiscordClient _);
```

- `UnregisterHandler` — Removes registered event subscriptions and detaches their runtime callbacks.

```csharp
internal void UnregisterHandler(DiscordClient _);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await commandHandlerService.HandleEventAsync(sender, eventArgs);
```

---

### CommandRegistry

**File:** `DC bot/Service/Core/CommandRegistry.cs`

**Purpose:** Builds a lazy, name-indexed snapshot of registered commands for discovery and dispatch.

**Implements / inherits:** `ICommandRegistry`.

**Properties:**

```csharp
public IReadOnlyCollection<ICommand> Commands { get; }
```

**Methods:**

- `TryGetCommand` — Looks up a command by name without rebuilding the lazy registry snapshot.

```csharp
public bool TryGetCommand(string commandName, out ICommand command);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = commandRegistry.TryGetCommand(commandName, out var command);
```

---

### CommandValidationService

**File:** `DC bot/Service/Core/CommandValidationService.cs`

**Purpose:** Parses command arguments and presents user-validation failures before execution.

**Implements / inherits:** `ICommandHelper`.

**Methods:**

- `TryGetArgumentAsync` — Resolves a required argument and presents usage guidance when absent.

```csharp
public async Task<string?> TryGetArgumentAsync(
    IDiscordMessage message,
    IResponseBuilder responseBuilder,
    ILogger logger,
    string commandName,
    CancellationToken cancellationToken = default);
```

- `TryParseSavePlaylistArguments` — Parses a playlist name and source argument while preserving quoted values.

```csharp
public async Task<(
    IDiscordMessage message,
    IResponseBuilder responseBuilder,
    ILogger logger,
    string commandName,
    CancellationToken cancellationToken = default);
```

- `TryValidateUserAsync` — Runs user validation and sends a localized failure response when validation fails.

```csharp
public async Task<UserValidationResult?> TryValidateUserAsync(
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    IDiscordMessage message,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await commandValidationService.TryGetArgumentAsync(
    message,
    responseBuilder,
    logger,
    commandName,
    cancellationToken);
```

---

### ValidationService

**File:** `DC bot/Service/Core/ValidationService.cs`

**Purpose:** Checks voice membership and Lavalink connection readiness before music operations are accepted.

**Implements / inherits:** `IValidationService`, `IUserValidationService`.

**Methods:**

- `ValidateConnectionAsync` — Validates Lavalink readiness using available player and remote voice-state evidence.

```csharp
public async Task<ConnectionValidationResult> ValidateConnectionAsync(
    ILavalinkPlayer connection,
    CancellationToken cancellationToken = default);
```

- `ValidateUserAsync` — Checks that the requesting Discord user may perform voice-bound music operations.

```csharp
public async Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message, CancellationToken cancellationToken = default);
```

- `IsBotUser` — Checks whether an event user is the currently authenticated bot.

```csharp
public bool IsBotUser(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await validationService.ValidateConnectionAsync(connection, cancellationToken);
```

---

## Models and Results

### CommandRegistrySnapshot

**File:** `DC bot/Service/Core/CommandRegistrySnapshot.cs`

**Purpose:** Keeps the materialized commands and their name lookup together after registry initialization.

**Record declaration:**

```csharp
internal sealed record CommandRegistrySnapshot(IReadOnlyList<ICommand> Commands, IReadOnlyDictionary<string, ICommand> Lookup);
```

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Exceptions/` — Defines the common exception boundary and feature-specific failure categories.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
