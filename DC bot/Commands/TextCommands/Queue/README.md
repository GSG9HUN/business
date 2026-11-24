# Queue

Exposes queue display, repetition, ordering, and recent batch history through text commands.

## Overview

- Queue presentation includes current-track context and waiting-track durations. Clear replies report the affected count from the committed update; shuffle replies include the changed snapshot size. Empty queues are warnings.

- HistoryCommand loads recent batches and delegates row formatting and safe delivery to IResponseBuilder.SendHistoryAsync.

- Canonical text names and matching slash roots/dispatch targets use the shared CommandNames constants. Existing slash group and subcommand routes retain their own vocabulary.

- Provides queue inspection, ordering, removal, repeat and batch-history commands.
- Uses one-based waiting positions and shared playback validation for mutations.
- Presents localized descriptions, usage, queue results and validation errors.
- Uses informational embeds for queue searches and empty read-only listings; confirmed queue changes retain success presentation.

## Classes

### RemoveCommand

**Files:**

- `RemoveCommand.cs`

**Purpose:** Exposes `remove 3` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the Discord message.

**Usage:** `<prefix>remove 3`.

---

### MoveCommand

**Files:**

- `MoveCommand.cs`

**Purpose:** Exposes `move 5 2` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the Discord message.

**Usage:** `<prefix>move 5 2`.

---

### SkipToCommand

**Files:**

- `SkipToCommand.cs`

**Purpose:** Exposes `skipto 5` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the Discord message.

**Usage:** `<prefix>skipto 5`.

---

### QueueFindCommand

**Files:**

- `QueueFindCommand.cs`

**Purpose:** Exposes `queuefind artist` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the Discord message.

**Usage:** `<prefix>queuefind artist`.

---

### ClearCommand

**File:** `DC bot/Commands/TextCommands/Queue/ClearCommand.cs`

**Purpose:** Handles the clear text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `clear`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
public string DescriptionKey { get; }
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
public string DescriptionKey { get; }
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
public string DescriptionKey { get; }
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
public string DescriptionKey { get; }
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
public string DescriptionKey { get; }
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

### QueueCommand

**File:** `DC bot/Commands/TextCommands/Queue/QueueCommand.cs`

**Purpose:** Handles the view queue text command through validation, service, and localized response boundaries.

Displays the complete waiting queue in stable snapshot pages with numbered tracks. Pages contain at most ten items and share Previous/Next buttons, page/item counts and a five-minute lifetime; one-page results omit buttons. Text and slash entry points use the same panel.

**Implements / inherits:** `ICommand`.

**Command identifier:** `queue`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
public string DescriptionKey { get; }
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
