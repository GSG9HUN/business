# Playback

Accepts playback control requests and explicit repeat-mode changes for an authorized guild.

## Overview

Preset persistence uses the dedicated `POST /api/guilds/{guildId}/playback/audio-effects/presets` endpoint with `name`, `commands` (draft setting edits, possibly empty) and `overwrite`. `POST /api/guilds/{guildId}/playback/audio-effects` rejects preset-save operations. Both routes retain authorization and durable HTTP 202 command acceptance; terminal bot completion determines success. The preset-save path persists a definition without committing filters to the live player.

`AudioEffectsHandlers.GetAsync` authorizes guild access and returns the last authoritative filter map, a ten-item preset page, and cached management permission. Null filters require a durable `show` synchronization. `ApplyAsync` accepts bounded effect operations and rejects preset saving; `SavePresetAsync` accepts a typed `SaveAudioEffectPresetRequest` and creates a save-only `audio_effects` command; voice, parameter ranges, node capabilities and live preset permissions are enforced by the bot before side effects. POST acceptance is not final audio confirmation.

`SetVolumeAsync` applies guild authorization and validates `VolumePercent` in 0-200 before enqueueing a durable `set_volume` command. The accepted response confirms persistence of the command, not the final audio change; the bot revalidates voice access and reports completion separately.

`SeekAsync` validates a nonnegative absolute position and nonempty playback attempt, performs the existing guild/premium access check, and enqueues `SeekPlaybackCommandPayload` as a durable `seek` command. Worker completion, not HTTP acceptance, establishes the playback result.

- Command execution requests become durable bot-control rows. SetRepeatModeAsync directly updates repeat flags and repeat-list storage through repositories.
- The repeat-mode endpoint has its own persistence path; it does not itself perform a Discord or Lavalink operation. Keep this distinction when changing bot and mobile behavior together.

## Classes

### PlaybackHandlers

**File:** `API/Handlers/Playback/PlaybackHandlers.cs`

**Purpose:** Accepts authorized playback commands and persists explicit repeat-mode changes.

**Methods:**

- `ExecuteAsync` — Persists an authorized playback command for asynchronous execution by the bot.

```csharp
public static async Task<IResult> ExecuteAsync(
    HttpContext httpContext,
    IBotControlCommandsRepository repository,
    IMobileAppUserRepository userRepository,
    CancellationToken ct);
```

- `SetRepeatModeAsync` — Validates the requested repeat mode and persists repeat flags and the repeat-list snapshot.

```csharp
public static async Task<IResult> SetRepeatModeAsync(
    HttpContext httpContext,
    SetRepeatModeRequest request,
    IMobileAppUserRepository userRepository,
    IPlaybackStateRepository playbackStateRepository,
    IQueueRepository queueRepository,
    IRepeatListRepository repeatListRepository,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await PlaybackHandlers.ExecuteAsync(httpContext, repository, userRepository, ct);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Requests/Playback/` — Defines client-supplied input for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/Interface/Service/Persistence/BotControl/` — Defines repository operations for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
