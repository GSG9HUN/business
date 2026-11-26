# Playback

Defines client-supplied input for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

Preset persistence uses the dedicated `POST /api/guilds/{guildId}/playback/audio-effects/presets` endpoint with `name`, `commands` (draft setting edits, possibly empty) and `overwrite`. `POST /api/guilds/{guildId}/playback/audio-effects` rejects preset-save operations. Both routes retain authorization and durable HTTP 202 command acceptance; terminal bot completion determines success. The preset-save path persists a definition without committing filters to the live player.

[`SetVolumeRequest.cs`](SetVolumeRequest.cs) carries one absolute `VolumePercent` in 0-200. The `/playback/volume` handler validates the range and requires guild access before creating a command.

- Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.
- Optional input can mean leave unchanged rather than reset. Request fields are not authorization evidence; the authenticated principal and stored guild access determine permission.
- SetRepeatModeRequest uses none, one, or all. The explicit repeat-mode endpoint updates persistence directly and is separate from commands dispatched to the bot worker.

## Models and Results

### SaveAudioEffectPresetRequest

**File:** `SaveAudioEffectPresetRequest.cs`

**Purpose:** Carries a guild-local preset name, zero to 39 draft setting edits and explicit overwrite consent. The save handler rejects player/preset actions, control characters, invalid names and oversized batches. The bot rechecks parameter ranges and live management/voice permissions before persistence.

---

### SeekPlaybackRequest

**Files:** `SeekPlaybackRequest.cs`

**Purpose:** Absolute position and captured playback attempt; validated by the seek handler before acceptance.

**Record declaration:**

```csharp
public sealed record SeekPlaybackRequest(int PositionSeconds, Guid ExpectedPlaybackOperationId);
```

---

### SetRepeatModeRequest

**File:** `API/Requests/Playback/SetRepeatModeRequest.cs`

**Purpose:** Carries client input for set repeat mode processing; handlers and endpoint filters validate its values.

**Record declaration:**

```csharp
public sealed record SetRepeatModeRequest(string Mode);
```

---

## Related Components

- `API/Requests/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
