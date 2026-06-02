# Utility Slash Commands

This folder contains general-purpose slash command adapters.

## Commands

### PingSlashCommand.cs

**Command:** `/ping`

**Delegates to:** `PingCommand`

**Behavior:** Executes the existing ping command pipeline and returns the configured ping response.

---

### HelpSlashCommand.cs

**Command:** `/help`

**Delegates to:** `HelpCommand`

**Behavior:** Executes the existing help command pipeline and lists registered text commands.

---

### LanguageSlashCommand.cs

**Command:** `/language language:<eng|hu>`

**Delegates to:** `LanguageCommand`

**Behavior:**

1. Uses DSharpPlus choices for supported languages.
2. Maps `eng` and `hu` choices to the text command argument.
3. Saves the guild language through the existing text command pipeline.

---

### TagSlashCommand.cs

**Command:** `/tag user:<member>`

**Delegates to:** `TagCommand`

**Behavior:**

1. Uses a Discord member option instead of free-text username matching.
2. Passes the selected member mention into the text command pipeline.
3. Reuses the localized tag response behavior from `TagCommand`.

## Related Components

- `../README.md` - slash command architecture
- `../../TextCommands/Utility/README.md` - text command behavior reused by these adapters
- `../../../Service/SlashCommands/SlashCommandExecutor.cs` - slash-to-text command execution
- `../../../localization/README.md` - localized response resources
