# Music Slash Commands

This folder contains slash command adapters for music playback control.

## Commands

### JoinSlashCommand.cs

**Command:** `/join`

**Delegates to:** `JoinCommand`

**Behavior:** Requires a guild context, defers the interaction, and joins the user's voice channel to start queued music
through the existing text command pipeline.

---

### PlaySlashCommand.cs

**Command:** `/play query:<url-or-search>`

**Delegates to:** `PlayCommand`

**Behavior:**

1. Requires a guild context.
2. Defers the interaction because track loading can take longer than a simple response.
3. Passes the query or URL as the text command argument.
4. Reuses the existing play pipeline for voice validation, source resolution, queueing, playback, and localized responses.

---

### PauseSlashCommand.cs

**Command:** `/pause`

**Delegates to:** `PauseCommand`

**Behavior:** Requires a guild context, defers the interaction, and pauses the current track through the existing text
command pipeline.

---

### ResumeSlashCommand.cs

**Command:** `/resume`

**Delegates to:** `ResumeCommand`

**Behavior:** Requires a guild context, defers the interaction, and resumes paused playback through the existing text
command pipeline.

---

### SkipSlashCommand.cs

**Command:** `/skip`

**Delegates to:** `SkipCommand`

**Behavior:** Requires a guild context, defers the interaction, and skips/stops the current track through the existing
text command pipeline.

---

### LeaveSlashCommand.cs

**Command:** `/leave`

**Delegates to:** `LeaveCommand`

**Behavior:** Requires a guild context, defers the interaction, and disconnects/cleans up playback state through the
existing text command pipeline.

## Related Components

- `../README.md` - slash command architecture
- `../../TextCommands/Music/README.md` - text command behavior reused by these adapters
- `../../../Service/SlashCommands/SlashCommandExecutor.cs` - slash-to-text command execution
- `../../../Service/Music/README.md` - music playback services
