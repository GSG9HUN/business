# SlashCommands

Exposes Discord application commands through DSharpPlus command modules.

## Overview

- Adapters build SlashCommandExecutionRequest values and delegate to ISlashCommandExecutor, which reuses the registered text command implementation.
- Interaction deferral, guild-only handling, and response wrapping belong to this adapter path. Avoid duplicating playback or playlist logic in slash modules.

## Subfolders

### Music/

Exposes voice join/leave and playback controls through slash commands.

**Files:**

- `JoinSlashCommand.cs`
- `LeaveSlashCommand.cs`
- `PauseSlashCommand.cs`
- `PlaySlashCommand.cs`
- `PreviousSlashCommand.cs`
- `ResumeSlashCommand.cs`
- `SkipSlashCommand.cs`

**Purpose:** DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.

---

### Playlist/

Exposes saved playlist creation, URL import, track editing, and queue loading through slash commands.

**Files:**

- `PlaylistSlashCommand.cs`

**Purpose:** DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.

---

### Queue/

Exposes queue display, repetition, ordering, and recent batch history through slash commands.

**Files:**

- `ClearSlashCommand.cs`
- `HistorySlashCommand.cs`
- `QueueSlashCommand.cs`
- `RepeatSlashCommand.cs`
- `ShuffleSlashCommand.cs`

**Purpose:** DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.

---

### Utility/

Exposes help, language selection, latency checks, and user tagging through slash commands.

**Files:**

- `HelpSlashCommand.cs`
- `LanguageSlashCommand.cs`
- `PingSlashCommand.cs`
- `TagSlashCommand.cs`

**Purpose:** DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.

---

## Related Components

- `DC bot/Commands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Commands/SlashCommands/Music/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/SlashCommands/Playlist/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/SlashCommands/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/SlashCommands/Utility/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
