# Queue

Exposes queue display, repetition, ordering, and recent batch history through slash commands.

## Overview

- Exposes queue inspection, ordering, removal, repeat and history as slash commands.
- Maps interaction options to shared text-command requests with guild validation.
- Uses one-based waiting positions for queue edits.

## Classes

### RemoveSlashCommand

**Files:**

- `RemoveSlashCommand.cs`

**Purpose:** Exposes `remove 3` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the adapted interaction; the attributed command method binds typed Discord options.

**Usage:** `/remove 3`.

---

### MoveSlashCommand

**Files:**

- `MoveSlashCommand.cs`

**Purpose:** Exposes `move 5 2` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the adapted interaction; the attributed command method binds typed Discord options.

**Usage:** `/move 5 2`.

---

### SkipToSlashCommand

**Files:**

- `SkipToSlashCommand.cs`

**Purpose:** Exposes `skipto 5` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the adapted interaction; the attributed command method binds typed Discord options.

**Usage:** `/skipto 5`.

---

### QueueFindSlashCommand

**Files:**

- `QueueFindSlashCommand.cs`

**Purpose:** Exposes `queuefind artist` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the adapted interaction; the attributed command method binds typed Discord options.

**Usage:** `/queuefind artist`.

---

### ClearSlashCommand

**File:** `DC bot/Commands/SlashCommands/Queue/ClearSlashCommand.cs`

**Purpose:** Adapts the clear slash command to shared command execution and interaction acknowledgement.

The confirmation-required response is a localized warning embed; confirmation and command execution rules remain the same.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public async Task ExecuteAsync(ISlashInteractionContext context, bool confirm);
```

- `Clear` — Adapts the clear slash route to its shared handler and interaction response policy.

```csharp
public Task Clear(
    SlashCommandContext context,
    [Parameter("confirm")][Description("Must be true to clear the queue")] bool confirm = false);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await clearSlashCommand.Clear(context, confirm);
```

---

### HistorySlashCommand

**File:** `DC bot/Commands/SlashCommands/Queue/HistorySlashCommand.cs`

**Purpose:** Adapts the history slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `History` — Adapts the history slash route to its shared handler and interaction response policy.

```csharp
public Task History(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await historySlashCommand.ExecuteAsync(context);
```

---

### QueueSlashCommand

**File:** `DC bot/Commands/SlashCommands/Queue/QueueSlashCommand.cs`

**Purpose:** Adapts the queue slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Queue` — Adapts the queue slash route to its shared handler and interaction response policy.

```csharp
public Task Queue(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await queueSlashCommand.ExecuteAsync(context);
```

---

### RepeatSlashCommand

**File:** `DC bot/Commands/SlashCommands/Queue/RepeatSlashCommand.cs`

**Purpose:** Adapts the repeat slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteListAsync` — Adapts the execute list async slash route to its shared handler and interaction response policy.

```csharp
public Task ExecuteListAsync(ISlashInteractionContext context);
```

- `ExecuteTrackAsync` — Adapts the execute track async slash route to its shared handler and interaction response policy.

```csharp
public Task ExecuteTrackAsync(ISlashInteractionContext context);
```

- `List` — Adapts the list slash route to its shared handler and interaction response policy.

```csharp
public Task List(SlashCommandContext context);
```

- `Track` — Adapts the track slash route to its shared handler and interaction response policy.

```csharp
public Task Track(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await repeatSlashCommand.ExecuteListAsync(context);
```

---

### ShuffleSlashCommand

**File:** `DC bot/Commands/SlashCommands/Queue/ShuffleSlashCommand.cs`

**Purpose:** Adapts the shuffle slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Shuffle` — Adapts the shuffle slash route to its shared handler and interaction response policy.

```csharp
public Task Shuffle(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await shuffleSlashCommand.ExecuteAsync(context);
```

---

## Related Components

- `DC bot/Commands/SlashCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Interface/Service/Localization/` — Defines translation lookup, formatting, and guild language selection.
- `DC bot/Interface/Service/SlashCommands/` — Connects DSharpPlus interactions to the shared text command pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
