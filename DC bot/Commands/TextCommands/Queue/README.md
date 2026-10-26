# Queue

Exposes queue display, repetition, ordering, and recent batch history through text commands.

## Overview

- CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.
- The history command lists recent batches without changing playback. Repeat-list targets the active batch, including tracks played earlier in that batch; it does not select arbitrary older history for replay.

## Classes

### ClearCommand

**File:** `DC bot/Commands/TextCommands/Queue/ClearCommand.cs`

**Purpose:** Handles the clear text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `clear`. Message routing applies the configured prefix.

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
await clearCommand.ExecuteAsync(message);
```

---

### HistoryCommand

**File:** `DC bot/Commands/TextCommands/Queue/HistoryCommand.cs`

**Purpose:** Reads recent playback batches and displays their status and track count. This is a read-only history scaffold; it does not replay an old batch or move the previous-track cursor.

**Implements / inherits:** `ICommand`.

**Command identifier:** `history`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Lists the guild's recent playback batches without requiring voice membership or changing playback state.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await historyCommand.ExecuteAsync(message);
```

---

### RepeatCommand

**File:** `DC bot/Commands/TextCommands/Queue/RepeatCommand.cs`

**Purpose:** Handles the repeat text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `repeat`. Message routing applies the configured prefix.

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
await repeatCommand.ExecuteAsync(message);
```

---

### RepeatListCommand

**File:** `DC bot/Commands/TextCommands/Queue/RepeatListCommand.cs`

**Purpose:** Handles the repeat list text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `repeatList`. Message routing applies the configured prefix.

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
await repeatListCommand.ExecuteAsync(message);
```

---

### ShuffleCommand

**File:** `DC bot/Commands/TextCommands/Queue/ShuffleCommand.cs`

**Purpose:** Handles the shuffle text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `shuffle`. Message routing applies the configured prefix.

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
await shuffleCommand.ExecuteAsync(message);
```

---

### ViewQueueCommand

**File:** `DC bot/Commands/TextCommands/Queue/ViewQueueCommand.cs`

**Purpose:** Handles the view queue text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `viewList`. Message routing applies the configured prefix.

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
await viewQueueCommand.ExecuteAsync(message);
```

---

## Related Components

- `DC bot/Commands/TextCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Interface/Core/` — Defines command lookup, argument helpers, and user/player validation boundaries.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
