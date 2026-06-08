# Queue Slash Commands

This folder contains slash command adapters for music queue management.

## Commands

### QueueSlashCommand.cs

**Command:** `/queue`

**Delegates to:** `ViewQueueCommand`

**Behavior:** Requires a guild context, defers the interaction, and displays the current queue through the existing text
command pipeline.

---

### ShuffleSlashCommand.cs

**Command:** `/shuffle`

**Delegates to:** `ShuffleCommand`

**Behavior:** Requires a guild context, defers the interaction, and shuffles the current queue through the existing text
command pipeline.

---

### RepeatSlashCommand.cs

**Commands:**

- `/repeat track` -> `RepeatCommand`
- `/repeat list` -> `RepeatListCommand`

**Behavior:** Uses DSharpPlus subcommands and forwards each subcommand to the matching text command name.

---

### ClearSlashCommand.cs

**Command:** `/clear confirm:<true|false>`

**Delegates to:** `ClearCommand` only when `confirm` is `true`.

**Behavior:**

1. Requires explicit confirmation before clearing queue state.
2. Sends a localized confirmation-required message when `confirm` is `false`.
3. Delegates to `ClearCommand` when `confirm` is `true`.

## Related Components

- `../README.md` - slash command architecture
- `../../TextCommands/Queue/README.md` - text command behavior reused by these adapters
- `../../../Service/SlashCommands/SlashCommandExecutor.cs` - slash-to-text command execution
- `../../../Service/Music/MusicServices/README.md` - queue and repeat services
