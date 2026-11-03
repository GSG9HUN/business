# Music

Exposes voice join/leave and playback controls through slash commands.

## Overview

- DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.

## Classes

### JoinSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/JoinSlashCommand.cs`

**Purpose:** Adapts the join slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Join` — Adapts the join slash route to its shared handler and interaction response policy.

```csharp
public Task Join(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await joinSlashCommand.ExecuteAsync(context);
```

---

### LeaveSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/LeaveSlashCommand.cs`

**Purpose:** Adapts the leave slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Leave` — Adapts the leave slash route to its shared handler and interaction response policy.

```csharp
public Task Leave(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await leaveSlashCommand.ExecuteAsync(context);
```

---

### PauseSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/PauseSlashCommand.cs`

**Purpose:** Adapts the pause slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Pause` — Adapts the pause slash route to its shared handler and interaction response policy.

```csharp
public Task Pause(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await pauseSlashCommand.ExecuteAsync(context);
```

---

### PlaySlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/PlaySlashCommand.cs`

**Purpose:** Adapts the play slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string query);
```

- `Play` — Adapts the play slash route to its shared handler and interaction response policy.

```csharp
public Task Play(SlashCommandContext context, [Parameter("query")][Description("URL or search query")] string query);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await playSlashCommand.ExecuteAsync(context, query);
```

---

### PreviousSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/PreviousSlashCommand.cs`

**Purpose:** Adapts the previous slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Previous` — Adapts the previous slash route to its shared handler and interaction response policy.

```csharp
public Task Previous(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await previousSlashCommand.ExecuteAsync(context);
```

---

### ResumeSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/ResumeSlashCommand.cs`

**Purpose:** Adapts the resume slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Resume` — Adapts the resume slash route to its shared handler and interaction response policy.

```csharp
public Task Resume(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await resumeSlashCommand.ExecuteAsync(context);
```

---

### SkipSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/SkipSlashCommand.cs`

**Purpose:** Adapts the skip slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Skip` — Adapts the skip slash route to its shared handler and interaction response policy.

```csharp
public Task Skip(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await skipSlashCommand.ExecuteAsync(context);
```

---

### TwentyFourSevenSlashCommand

**File:** `DC bot/Commands/SlashCommands/Music/TwentyFourSevenSlashCommand.cs`

**Purpose:** Adapts the registered `/24-7` command to the shared text command with a required boolean option.

**Methods:**

```csharp
public Task TwentyFourSeven(SlashCommandContext context, bool enabled);
public Task ExecuteAsync(
    ISlashInteractionContext context,
    bool enabled,
    CancellationToken cancellationToken = default);
```

**Usage:**

```text
/24-7 enabled:true
/24-7 enabled:false
```

**Validation Checks:**

- Require a guild and defer the interaction before executing the shared command.
- Forward the boolean as `true` or `false` to `TwentyFourSevenCommand`.
- Register both the adapter and its command type through `CommandServiceCollectionExtensions`.
- The adapter changes the shared preference. The separate persistent voice worker handles automatic rejoining and checkpoint restoration.

---

## Related Components

- `DC bot/Commands/SlashCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/SlashCommands/` — Connects DSharpPlus interactions to the shared text command pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
