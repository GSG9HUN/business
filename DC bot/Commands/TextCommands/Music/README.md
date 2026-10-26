# Music

Exposes voice join/leave and playback controls through text commands.

## Overview

- CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.
- Repeated previous commands navigate only the active canonical batch. The command reports when no earlier eligible item exists; history is a separate read-only command.

## Classes

### JoinCommand

**File:** `DC bot/Commands/TextCommands/Music/JoinCommand.cs`

**Purpose:** Handles the join text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `join`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await joinCommand.ExecuteAsync(message);
```

---

### LeaveCommand

**File:** `DC bot/Commands/TextCommands/Music/LeaveCommand.cs`

**Purpose:** Handles the leave text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `leave`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await leaveCommand.ExecuteAsync(message);
```

---

### PauseCommand

**File:** `DC bot/Commands/TextCommands/Music/PauseCommand.cs`

**Purpose:** Handles the pause text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `pause`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await pauseCommand.ExecuteAsync(message);
```

---

### PlayCommand

**File:** `DC bot/Commands/TextCommands/Music/PlayCommand.cs`

**Purpose:** Handles the play text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `play`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await playCommand.ExecuteAsync(message);
```

---

### PreviousCommand

**File:** `DC bot/Commands/TextCommands/Music/PreviousCommand.cs`

**Purpose:** Handles the previous text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `previous`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await previousCommand.ExecuteAsync(message);
```

---

### ResumeCommand

**File:** `DC bot/Commands/TextCommands/Music/ResumeCommand.cs`

**Purpose:** Handles the resume text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `resume`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await resumeCommand.ExecuteAsync(message);
```

---

### SkipCommand

**File:** `DC bot/Commands/TextCommands/Music/SkipCommand.cs`

**Purpose:** Handles the skip text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `skip`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await skipCommand.ExecuteAsync(message);
```

---

## Persistent Voice Command

### TwentyFourSevenCommand

**File:** `TwentyFourSevenCommand.cs`

**Purpose:** Displays or changes the guild's saved twenty-four-seven voice preferences.

**Methods:**

```csharp
public Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

```text
!24-7
!24-7 true
!24-7 false
```

The first form displays the saved mode. Enabling validates the caller's voice membership and establishes or validates a player before saving its channel. Disabling only changes the saved flag and retains preferred channels.

**Validation Checks:**

- Ignore bot authors and reject non-guild messages.
- Accept only one boolean argument when changing the mode.
- Status and disable operations do not require voice membership.
- Join validation failures use the existing validation response; unexpected failures produce a localized error.
- The separate persistent voice worker monitors enabled guilds and restores confirmed playback checkpoints without requiring another listener.
- `leave` routes through `PersistentVoiceLeaveService`: save disabled mode first, then complete owned cleanup and close the batch. Cancelling the command wait does not abandon admitted leave.

---

## Related Components

- `DC bot/Commands/TextCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Interface/Core/` — Defines command lookup, argument helpers, and user/player validation boundaries.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
