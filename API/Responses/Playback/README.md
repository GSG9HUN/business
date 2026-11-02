# Playback

Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### PlaybackStatusResponse

**File:** `API/Responses/Playback/PlaybackStatusResponse.cs`

**Purpose:** Carries the playback status payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record PlaybackStatusResponse(
    string GuildId,
    string GuildName,
    string? GuildIconUrl,
    GuildBotStatusResponse BotStatus,
    PlaybackTrackResponse? CurrentTrack,
    bool IsPlaying,
    bool IsPaused,
    int PositionSeconds,
    int QueueTrackCount,
    bool IsRepeating,
    bool IsRepeatingList,
    DateTimeOffset UpdatedAtUtc);
```

---

### PlaybackTrackResponse

**File:** `API/Responses/Playback/PlaybackTrackResponse.cs`

**Purpose:** Carries the playback track payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record PlaybackTrackResponse(
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri,
    string? RequestedBy);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Responses/Guilds/` — Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
