# Music

Exposes voice join/leave and playback controls through slash commands.

## Overview

- Exposes typed Discord interaction options for playback, filters, discovery and guild music settings.
- Builds `SlashCommandExecutionRequest` values for shared text-command execution.
- Controls interaction acknowledgement while services enforce playback ownership and permissions.

## Classes

### MusicStatusSlashCommand

**Purpose:** Displays playback, voice and text destinations, audio settings, queue/repeat, votes, DJ role, 24/7 and the empty-channel countdown.

**Usage:** `/music-status`.

---

### MusicConfigSlashCommand

**Purpose:** Shows and changes guild vote-skip and DJ-role settings. Changes require Manage Guild and do not require a voice connection.

**Usage:** `/music-config show`, `/music-config vote-skip enabled:true`, `/music-config dj-role role:DJ`. Omitting the role clears it; Discord supplies a native role picker.

---

### KaraokeSlashCommand

**Files:**

- `KaraokeSlashCommand.cs`

**Purpose:** Routes /karaoke mode:on or mode:off to shared localized text behavior. FiltersSlashCommand also exposes Advanced(effect, values) and Custom(action, name, overwrite).

---

### LyricsSlashCommand

**Files:**

- `LyricsSlashCommand.cs`

**Purpose:** Routes `lyrics [query] [--page N]` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` dispatches adapted interaction arguments; attributed methods bind the typed slash options.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### FavoriteSlashCommand

**Files:**

- `FavoriteSlashCommand.cs`

**Purpose:** Routes `favorite add/list/remove/play` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` dispatches adapted interaction arguments; attributed methods bind the typed slash options.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### FavouriteSlashCommand

**Files:**

- `FavouriteSlashCommand.cs`

**Purpose:** Routes `favourite add/list/remove/play` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` dispatches adapted interaction arguments; attributed methods bind the typed slash options.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### RecommendationsSlashCommand

**Files:**

- `RecommendationsSlashCommand.cs`

**Purpose:** Routes `recommendations [query]` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` dispatches adapted interaction arguments; attributed methods bind the typed slash options.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### AutoplaySlashCommand

**Files:**

- `AutoplaySlashCommand.cs`

**Purpose:** Routes `autoplay on/off/status` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` dispatches adapted interaction arguments; attributed methods bind the typed slash options.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### SearchPickerSlashCommand

**Files:**

- `SearchPickerSlashCommand.cs`

**Purpose:** Routes `searchpicker on/off/status` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` dispatches adapted interaction arguments; attributed methods bind the typed slash options.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### PlayNextSlashCommand

**Files:**

- `PlayNextSlashCommand.cs`

**Purpose:** Exposes `playnext <query or URL>` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the adapted interaction; the attributed command method binds typed Discord options.

**Usage:** `/playnext <query or URL>`.

---

### FiltersSlashCommand

**Files:**

- `FiltersSlashCommand.cs`

**Purpose:** Inspects confirmed settings, lists supported presets, selects a preset, or resets effects. Uses the existing shared text-command executor.

**Usage:** `/filters show|list|reset`, `/filters preset name:<name>`, `/filters advanced effect:<effect> values:<values|off>` and `/filters custom action:<save|list|apply|delete> name:<name|page> overwrite:<bool>`. Omitted advanced values inspect current state.

Read-only inspection and preset listing do not require voice membership. Changes require the bot voice channel. Preset save/overwrite/delete requires Manage Guild; saving also captures current connected-player state.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string argument)
```

**Usage:**

```text
/filters [show|list|reset|preset <name>]
```

---

### BassBoostSlashCommand

**Files:**

- `BassBoostSlashCommand.cs`

**Purpose:** Replaces only the equalizer curve using off, low, medium, or high. Internal gain and unrelated effects remain unchanged. Uses the existing shared text-command executor.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/bassboost [off|low|medium|high]
```

---

### SpeedSlashCommand

**Files:**

- `SpeedSlashCommand.cs`

**Purpose:** Changes only timescale speed between 0.5 and 2.0 while retaining pitch, rate, and other effects. Uses the existing shared text-command executor.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/speed [0.5-2.0]
```

---

### PitchSlashCommand

**Files:**

- `PitchSlashCommand.cs`

**Purpose:** Changes only timescale pitch between 0.5 and 2.0 while retaining speed, rate, and other effects. Uses the existing shared text-command executor.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/pitch [0.5-2.0]
```

---

### NowPlayingSlashCommand

**Files:**

- `NowPlayingSlashCommand.cs`

**Purpose:** Displays the current track, requester, source, position, state, repeat, volume, and configured effects. Uses the existing SlashCommandExecutor and the matching text command.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context)
```

**Usage:**

```text
/nowplaying
```

---

### VolumeSlashCommand

**Files:**

- `VolumeSlashCommand.cs`

**Purpose:** Reads connected player volume without an argument or changes it in the bot voice channel. Levels above 100% are marked as amplified. Uses the existing SlashCommandExecutor and the matching text command.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/volume [0-200]
```

---

### SeekSlashCommand

**Files:**

- `SeekSlashCommand.cs`

**Purpose:** Moves to an absolute position before the track end while retaining pause and queue identity. Uses the existing SlashCommandExecutor and the matching text command.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/seek <seconds|mm:ss|hh:mm:ss>
```

---

### ForwardSlashCommand

**Files:**

- `ForwardSlashCommand.cs`

**Purpose:** Moves forward by nonnegative integer seconds, clamping before the end without dispatching skip. Uses the existing SlashCommandExecutor and the matching text command.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/forward <seconds>
```

---

### RewindSlashCommand

**Files:**

- `RewindSlashCommand.cs`

**Purpose:** Moves backward by nonnegative integer seconds, clamping at zero. Uses the existing SlashCommandExecutor and the matching text command.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
```

**Usage:**

```text
/rewind <seconds>
```

---

### RestartSlashCommand

**Files:**

- `RestartSlashCommand.cs`

**Purpose:** Seeks the current track to zero while retaining its history item and pause state. Uses the existing SlashCommandExecutor and the matching text command.

**Methods:**

```csharp
public Task ExecuteAsync(ISlashInteractionContext context)
```

**Usage:**

```text
/restart
```

---

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

The requester and configured DJ can skip immediately from the bot voice channel. Other human listeners cast one vote per track when guild voting is enabled. Responses distinguish a recorded vote from a completed skip.

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
