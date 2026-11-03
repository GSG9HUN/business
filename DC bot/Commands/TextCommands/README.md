# TextCommands

Implements commands selected by the configured message prefix and ICommandRegistry.

## Overview

- CommandHandlerService resolves a command, wraps its Discord message, and invokes ExecuteAsync. Commands validate the required user context and delegate to services.
- Commands expose a name and localized description. The same implementations are reused by slash and selected mobile command paths.

## Subfolders

### Music/

Exposes voice join/leave and playback controls through text commands.

**Files:**

- `JoinCommand.cs`
- `LeaveCommand.cs`
- `PauseCommand.cs`
- `PlayCommand.cs`
- `PreviousCommand.cs`
- `ResumeCommand.cs`
- `SkipCommand.cs`

**Purpose:** CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.

---

### Playlist/

Exposes saved playlist creation, URL import, track editing, and queue loading through text commands.

**Files:**

- `AddSongToPlaylistCommand.cs`
- `CreatePlaylistCommand.cs`
- `DeletePlaylistCommand.cs`
- `ListPlaylistsCommand.cs`
- `LoadPlaylistCommand.cs`
- `RemoveSongFromPlaylistCommand.cs`
- `RenamePlaylistCommand.cs`
- `SavePlaylistCommand.cs`
- `ViewPlaylistCommand.cs`

**Purpose:** CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.

---

### Queue/

Exposes queue display, repetition, ordering, and recent batch history through text commands.

**Files:**

- `ClearCommand.cs`
- `HistoryCommand.cs`
- `RepeatCommand.cs`
- `RepeatListCommand.cs`
- `ShuffleCommand.cs`
- `ViewQueueCommand.cs`

**Purpose:** CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.

---

### Utility/

Exposes help, language selection, latency checks, and user tagging through text commands.

**Files:**

- `HelpCommand.cs`
- `LanguageCommand.cs`
- `PingCommand.cs`
- `TagCommand.cs`

**Purpose:** CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.

---

## Related Components

- `DC bot/Commands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Commands/TextCommands/Music/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/TextCommands/Playlist/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/TextCommands/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/TextCommands/Utility/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
