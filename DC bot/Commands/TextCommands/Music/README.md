# Music

Exposes voice join/leave and playback controls through text commands.

## Overview

- Parses playback, audio-filter, discovery and guild-setting commands selected by the message prefix.
- Uses shared validation, playback ownership and localized response boundaries.
- Provides description keys and prefix-aware usage for command help and argument errors.

## Classes

### TwentyFourSevenCommand

**Purpose:** Shows or changes saved 24/7 preferences.

**File:** `TwentyFourSevenCommand.cs`.

**Usage:** `24-7 [true|false]`, with the configured prefix.

**Validation Checks:**

- Ignore bot authors and reject non-guild messages.
- Enabling validates voice membership and joins before saving preferences.
- Status/disable does not require voice membership; disabling retains preferred channels.
- Explicit leave disables persistent voice before full cleanup; admitted cleanup continues independently of caller cancellation.

---

### MusicStatusCommand

**Purpose:** Displays playback, voice and text destinations, audio settings, queue/repeat, votes, DJ role, 24/7 and the empty-channel countdown.

**Usage:** `music-status`, with the configured prefix. Unexpected arguments return usage.

---

### MusicConfigCommand

**Purpose:** Shows and changes guild vote-skip and DJ-role settings. Changes require Manage Guild and do not require a voice connection.

**Usage:** `music-config show`, `music-config vote-skip on|off`, `music-config dj-role @role|ID|clear`. Text commands use the configured prefix.

---

### KaraokeCommand

**Files:**

- `KaraokeCommand.cs`

**Purpose:** Routes karaoke [on|off] to advanced karaoke composition through the existing facade. No argument inspects state; a change requires the bot voice channel.

---

### LyricsCommand

**Files:**

- `LyricsCommand.cs`

**Purpose:** Routes `lyrics [query] [--page N]` to the shared discovery service and localized result boundary.

Renders continuous lyrics as line-aware pages in one embed. `--page N` chooses the starting page; buttons reuse the loaded lyrics without repeating the provider request.

**Methods:** `ExecuteAsync` handles the Discord message and its arguments.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### FavoriteCommand

**Files:**

- `FavoriteCommand.cs`

**Purpose:** Routes `favorite add/list/remove/play` to the shared discovery service and localized result boundary.

List results use caller-owned buttons and lazy ten-entry database pages. `favorite list N` chooses the starting page; add/remove/play keep their ordinary localized result embeds.

**Methods:** `ExecuteAsync` handles the Discord message and its arguments.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### FavouriteCommand

**Files:**

- `FavouriteCommand.cs`

**Purpose:** Routes `favourite add/list/remove/play` to the shared discovery service and localized result boundary.

Uses the same paginated personal list as `favorite`, including the optional initial page argument.

**Methods:** `ExecuteAsync` handles the Discord message and its arguments.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### RecommendationsCommand

**Files:**

- `RecommendationsCommand.cs`

**Purpose:** Routes `recommendations [query]` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` handles the Discord message and its arguments.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### AutoplayCommand

**Files:**

- `AutoplayCommand.cs`

**Purpose:** Routes `autoplay on/off/status` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` handles the Discord message and its arguments.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### SearchPickerCommand

**Files:**

- `SearchPickerCommand.cs`

**Purpose:** Routes `searchpicker on/off/status` to the shared discovery service and localized result boundary.

**Methods:** `ExecuteAsync` handles the Discord message and its arguments.

**Validation Checks:** Personal favorite ownership, autoplay voice mutation rules and picker user/guild/message/session binding are enforced by services.

---

### PlayNextCommand

**Files:**

- `PlayNextCommand.cs`

**Purpose:** Exposes `playnext <query or URL>` through existing validation, localized results and shared playback/queue services.

**Methods:** `ExecuteAsync` handles the Discord message.

**Usage:** `<prefix>playnext <query or URL>`.

---

### FiltersCommand

**Files:**

- `FiltersCommand.cs`

**Purpose:** Inspects confirmed settings, lists supported presets, selects a preset, or resets effects.

`filters custom list [page]` uses lazy ten-entry guild-scoped database pages. Inspecting or applying filters retains the existing result presentation.

**Usage:** `filters [show|list|reset|preset <name>]`, `filters advanced help`, `filters advanced <effect> [values|off]`, and `filters custom save <name> [overwrite]|list [page]|apply <name>|delete <name>`.

Read-only inspection and preset listing do not require voice membership. Changes require the bot voice channel. Preset save/overwrite/delete requires Manage Guild; saving also captures current connected-player state.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>filters [show|list|reset|preset <name>]
```

---

### BassBoostCommand

**Files:**

- `BassBoostCommand.cs`

**Purpose:** Replaces only the equalizer curve using off, low, medium, or high. Internal gain and unrelated effects remain unchanged.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>bassboost [off|low|medium|high]
```

---

### SpeedCommand

**Files:**

- `SpeedCommand.cs`

**Purpose:** Changes only timescale speed between 0.5 and 2.0 while retaining pitch, rate, and other effects.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>speed [0.5-2.0]
```

---

### PitchCommand

**Files:**

- `PitchCommand.cs`

**Purpose:** Changes only timescale pitch between 0.5 and 2.0 while retaining speed, rate, and other effects.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>pitch [0.5-2.0]
```

---

### NowPlayingCommand

**Files:**

- `NowPlayingCommand.cs`

**Purpose:** Displays the current track, requester, source, position, state, repeat, volume, and configured effects.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>nowplaying
```

---

### VolumeCommand

**Files:**

- `VolumeCommand.cs`

**Purpose:** Reads connected player volume without an argument or changes it in the bot voice channel. Levels above 100% are marked as amplified.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>volume [0-200]
```

---

### SeekCommand

**Files:**

- `SeekCommand.cs`

**Purpose:** Moves to an absolute position before the track end while retaining pause and queue identity.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>seek <seconds|mm:ss|hh:mm:ss>
```

---

### ForwardCommand

**Files:**

- `ForwardCommand.cs`

**Purpose:** Moves forward by nonnegative integer seconds, clamping before the end without dispatching skip.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>forward <seconds>
```

---

### RewindCommand

**Files:**

- `RewindCommand.cs`

**Purpose:** Moves backward by nonnegative integer seconds, clamping at zero.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>rewind <seconds>
```

---

### RestartCommand

**Files:**

- `RestartCommand.cs`

**Purpose:** Seeks the current track to zero while retaining its history item and pause state.

**Methods:**

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
```

**Usage:**

```text
<prefix>restart
```

---

### JoinCommand

**File:** `DC bot/Commands/TextCommands/Music/JoinCommand.cs`

**Purpose:** Handles the join text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `join`. Message routing applies the configured prefix.

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
await joinCommand.ExecuteAsync(message);
```

---

### LeaveCommand

**File:** `DC bot/Commands/TextCommands/Music/LeaveCommand.cs`

**Purpose:** Handles the leave text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `leave`. Message routing applies the configured prefix.

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
await leaveCommand.ExecuteAsync(message);
```

---

### PauseCommand

**File:** `DC bot/Commands/TextCommands/Music/PauseCommand.cs`

**Purpose:** Handles the pause text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `pause`. Message routing applies the configured prefix.

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
await pauseCommand.ExecuteAsync(message);
```

---

### PlayCommand

**File:** `DC bot/Commands/TextCommands/Music/PlayCommand.cs`

**Purpose:** Handles the play text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `play`. Message routing applies the configured prefix.

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
await playCommand.ExecuteAsync(message);
```

---

### PreviousCommand

**File:** `DC bot/Commands/TextCommands/Music/PreviousCommand.cs`

**Purpose:** Handles the previous text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `previous`. Message routing applies the configured prefix.

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
await previousCommand.ExecuteAsync(message);
```

---

### ResumeCommand

**File:** `DC bot/Commands/TextCommands/Music/ResumeCommand.cs`

**Purpose:** Handles the resume text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `resume`. Message routing applies the configured prefix.

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
await resumeCommand.ExecuteAsync(message);
```

---

### SkipCommand

**File:** `DC bot/Commands/TextCommands/Music/SkipCommand.cs`

**Purpose:** Handles the skip text command through validation, service, and localized response boundaries.

The requester and configured DJ can skip immediately from the bot voice channel. Other human listeners cast one vote per track when guild voting is enabled. Responses distinguish a recorded vote from a completed skip.

**Implements / inherits:** `ICommand`.

**Command identifier:** `skip`. Message routing applies the configured prefix.

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
await skipCommand.ExecuteAsync(message);
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
