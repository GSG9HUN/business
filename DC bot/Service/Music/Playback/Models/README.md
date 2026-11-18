# Models

Contains SDK-dependent data snapshots used by playback implementations.

## Overview

- Stores playback-local filter snapshots independently of mutable SDK collections.
- Supports complete filter composition, confirmation and recovery.

## Playback Models

### PlaybackFilterState

**Files:**

- `PlaybackFilterState.cs`

**Purpose:** Retains a complete filter configuration for composition, acknowledgement tracking, and recovery.

**Properties:**

```csharp
public PlayerFilterMapModel Filters { get; init; }
```

The SDK model carries internal filter volume, equalizer bands, timescale, karaoke, rotation, channel mix, low-pass, tremolo, vibrato, distortion, and additional plugin filters. The service clones plugin JSON and stores it in an immutable dictionary; equalizer bands use the SDK immutable array.

**Validation Checks:** The service validates user multipliers, bass levels, node capabilities, and current session ownership before applying a snapshot. The snapshot does not perform I/O.

---

## Related Components

- `DC bot/Service/Music/Playback/PlaybackFilterService.cs` - Creates, composes, and confirms snapshots.
- `DC bot/Service/Music/Lifecycle/Sessions/GuildPlaybackSessions.cs` - Retains confirmed and pending snapshots under the guild lock.
- `DC bot/Interface/Service/Music/Playback/IPlaybackFilterService.cs` - Exposes filter operations through existing playback results.
