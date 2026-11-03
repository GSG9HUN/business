# Commands

Groups prefix-based text commands and slash command adapters.

## Overview

- Text commands implement ICommand and reuse services and localized responses. Slash adapters enter the same command pipeline through SlashCommandExecutor.
- Keep command parsing and presentation at this boundary; guild sessions, transport calls, and persistence decisions belong to services.

## Subfolders

### SlashCommands/

Exposes Discord application commands through DSharpPlus command modules.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** Adapters build SlashCommandExecutionRequest values and delegate to ISlashCommandExecutor, which reuses the registered text command implementation.

---

### TextCommands/

Implements commands selected by the configured message prefix and ICommandRegistry.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** CommandHandlerService resolves a command, wraps its Discord message, and invokes ExecuteAsync. Commands validate the required user context and delegate to services.

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Commands/SlashCommands/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Commands/TextCommands/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
